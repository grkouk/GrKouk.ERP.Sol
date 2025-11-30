using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public class TransactorTransactionServiceV2 : ITransactorTransactionService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<TransactorTransactionServiceV2> _logger;
    private readonly ICFATransactionService _cfaTransSrv;
    private const string _defaultSectionCode = GrKouk.Erp.Definitions.Constants.SectionTransactorTransactions;
    public TransactorTransactionServiceV2(ApiDbContext context, ILogger<TransactorTransactionServiceV2> logger, ICFATransactionService cfaTransSrv)
    {
        _context = context;
        _logger = logger;
        _cfaTransSrv = cfaTransSrv;
    }


    public async Task<ServiceResult> AddTransactorTransaction(TransactorTransCreateDto itemVm,string  callerSectionCode=null)
    {
        int fiscalPeriodId;

        #region Fiscal Period
        // Check if we already have a fiscal period for this date
        // This should be true if this is called from another higher update service
        // Example from the Document Update Service
        if (!(itemVm.FiscalPeriodId > 0))
        {
            var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, itemVm.TransDate);
            if (fiscalPeriod == null)
            {
                return ServiceResult.Error("No Fiscal Period covers Transaction Date", "BADREQUEST");
            }
            fiscalPeriodId = fiscalPeriod.Id;    
        }
        else
        {
            fiscalPeriodId = itemVm.FiscalPeriodId;
        }
        
        #endregion

        var spTransaction = new TransactorTransaction
        {
            TransDate = itemVm.TransDate,
            TransTransactorDocSeriesId = itemVm.TransTransactorDocSeriesId,
            TransTransactorDocTypeId = itemVm.TransTransactorDocTypeId,
            TransactorId = itemVm.TransactorId,
            TransRefCode = itemVm.TransRefCode,
            CompanyId = itemVm.CompanyId,
            CfAccountId = itemVm.CfAccountId,
            AmountNet = itemVm.AmountNet,
            AmountDiscount = itemVm.AmountDiscount,
            AmountFpa = itemVm.AmountFpa,
            Etiology = itemVm.Etiology,
            FpaRate = itemVm.FpaRate,
            DiscountRate = itemVm.DiscountRate,

        };
        var docSeries = await
            _context.TransTransactorDocSeriesDefs.SingleOrDefaultAsync(m =>
                m.Id == itemVm.TransTransactorDocSeriesId);

        if (docSeries is null)
        {
            return ServiceResult.Error("Δεν βρέθηκε η σειρά του παραστατικού", "BADREQUEST");
        }
        await _context.Entry(docSeries).Reference(t => t.TransTransactorDocTypeDef).LoadAsync();

        var docTypeDef = docSeries.TransTransactorDocTypeDef;
        await _context.Entry(docTypeDef)
            .Reference(t => t.TransTransactorDef)
            .LoadAsync();

        var transTransactorDef = docTypeDef.TransTransactorDef;
        int sectionId;
        #region Section Management
      
        if (docTypeDef.SectionId == 0)
        {
            var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _defaultSectionCode);
            if (sectn == null)
            {
                return ServiceResult.Error("Δεν υπάρχει το Section", "BADREQUEST");
            }
            sectionId = sectn.Id;
        }
        else
        {
            sectionId = docTypeDef.SectionId;
        }
        #endregion

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            spTransaction.SectionId = sectionId;
            spTransaction.TransTransactorDocTypeId = docSeries.TransTransactorDocTypeDefId;
            spTransaction.FiscalPeriodId = fiscalPeriodId;
            spTransaction.FinancialAction = transTransactorDef.FinancialTransAction;
            ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, spTransaction);
            await _context.TransactorTransactions.AddAsync(spTransaction);
            await _context.SaveChangesAsync();
            //Retrieve new id of the transaction
            var NewDocId = _context.Entry(spTransaction).Entity.Id;
            
            
            if (itemVm.CfAccountId>0)
            {
                //TODO: Use CFA transaction service to create CFA transaction
                var cfaSeriesId = docSeries.DefaultCfaTransSeriesId;
                if (cfaSeriesId>0)
                {
                    var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(cfaSeriesId);
                    if (cfaSeries!=null)
                    {
                        await _context.Entry(cfaSeries)
                            .Reference(t => t.CashFlowDocTypeDefinition)
                            .LoadAsync();

                        var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                        if (cfaType!=null)
                        {
                            await _context.Entry(cfaType)
                                .Reference(t => t.CashFlowTransactionDefinition)
                                .LoadAsync();
                            var transactor = await _context.Transactors
                                .Where(p => p.Id == itemVm.TransactorId)
                                .AsNoTracking()
                                .SingleOrDefaultAsync();
                            var etiology =
                                $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {itemVm.Etiology} ";

                            var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                            var cfaTrans = new CfaTransactionCreateDto() {
                                TransDate = itemVm.TransDate,
                                CashFlowAccountId = itemVm.CfAccountId,
                                CompanyId = itemVm.CompanyId,
                                DocSeriesId = cfaSeries.Id,
                                DocTypeId = cfaType.Id,
                                Etiology = etiology,
                                FiscalPeriodId = spTransaction.FiscalPeriodId,
                                CreatorSectionId = itemVm.CreatorSectionId,
                                CreatorId = itemVm.CreatorId,
                                TransRefCode = spTransaction.TransRefCode,
                                Amount = itemVm.AmountSum,
                               
                            };
                            var cfaResult = await _cfaTransSrv.AddCFATransaction(cfaTrans, null);
                            if (!cfaResult.Success)
                            {
                                if (ownsTransaction) await transaction.RollbackAsync();
                                return ServiceResult.Error($"CashFlow Account Transaction failed to update", "BADREQUEST");
                            }
                            
                            // //If itemvn.creatorid is 0 then it is a transaction from transactor transactions directly
                            // //so pass the creator id and section id from the transaction 
                            // if (itemVm.CreatorId == 0)
                            // {
                            //     cfaTrans.CreatorSectionId = sectionId;
                            //     cfaTrans.CreatorId = spTransaction.Id;
                            // // }
                            // ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                            // await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                            // await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            //await _context.SaveChangesAsync(); 
            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}", "BADREQUEST");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
        return ServiceResult.Ok(spTransaction);
    }

    public async Task<ServiceResult> ModifyTransactorTransaction(TransactorTransModifyDto itemVm, string callerSectionCode = null)
    {
        int fiscalPeriodId;
        #region Fiscal Period
        // Check if we already have a fiscal period for this date
        // This should be true if this is called from another higher update service
        // Example from the Document Update Service
        if (!(itemVm.FiscalPeriodId > 0))
        {
            var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, itemVm.TransDate);
            if (fiscalPeriod == null)
            {
                return ServiceResult.Error("No Fiscal Period covers Transaction Date", "BADREQUEST");
            }
            fiscalPeriodId = fiscalPeriod.Id;    
        }
        else
        {
            fiscalPeriodId = itemVm.FiscalPeriodId;
        }
        #endregion

        // Load doc series and definitions
        var docSeries = await _context.TransTransactorDocSeriesDefs
            .SingleOrDefaultAsync(m => m.Id == itemVm.TransTransactorDocSeriesId);
        if (docSeries is null)
        {
            return ServiceResult.Error("Δεν βρέθηκε η σειρά παραστατικο", "BADREQUEST");
        }
        await _context.Entry(docSeries).Reference(t => t.TransTransactorDocTypeDef).LoadAsync();
        var docTypeDef = docSeries.TransTransactorDocTypeDef;
        await _context.Entry(docTypeDef).Reference(t => t.TransTransactorDef).LoadAsync();
        var transTransactorDef = docTypeDef.TransTransactorDef;

        #region Section Management

        int sectionId ;
      
        if (docTypeDef.SectionId == 0)
        {
            var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _defaultSectionCode);
            if (sectn == null)
            {
                return ServiceResult.Error("Δεν υπάρχει το Section", "BADREQUEST");
            }
            sectionId = sectn.Id;
        }
        else
        {
            sectionId = docTypeDef.SectionId;
        }
        #endregion
        // Get old transaction (for old section id) and the entity to update
        var spOldTrans = await _context.TransactorTransactions
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == itemVm.Id);
        if (spOldTrans == null)
        {
            return ServiceResult.Error("Η συναλλαγή δεν βρέθηκε", "BADREQUEST");
        }
        int oldTransSectionId = spOldTrans.SectionId == 0 ? sectionId : spOldTrans.SectionId;
        
        int oldTransCreatorSectionId = spOldTrans.CreatorSectionId;
        int oldTransCreatorId = spOldTrans.CreatorId;
        
        var spTransaction = await _context.TransactorTransactions
            .SingleOrDefaultAsync(p => p.Id == itemVm.Id);
        if (spTransaction == null)
        {
            return ServiceResult.Error("Η συναλλαγή δεν βρέθηκε", "BADREQUEST");
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            // Update fields
            spTransaction.TransDate = itemVm.TransDate;
            spTransaction.TransTransactorDocSeriesId = itemVm.TransTransactorDocSeriesId;
            spTransaction.TransTransactorDocTypeId = docSeries.TransTransactorDocTypeDefId;
            spTransaction.TransactorId = itemVm.TransactorId;
            spTransaction.TransRefCode = itemVm.TransRefCode;
            spTransaction.CompanyId = itemVm.CompanyId;
            spTransaction.CfAccountId = itemVm.CfAccountId;
            spTransaction.AmountNet = itemVm.AmountNet;
            spTransaction.AmountDiscount = itemVm.AmountDiscount;
            spTransaction.AmountFpa = itemVm.AmountFpa;
            spTransaction.Etiology = itemVm.Etiology;
            spTransaction.FpaRate = itemVm.FpaRate;
            spTransaction.DiscountRate = itemVm.DiscountRate;
            spTransaction.SectionId = sectionId;
            spTransaction.CreatorId = itemVm.CreatorId;
            spTransaction.CreatorSectionId = itemVm.CreatorSectionId;
            spTransaction.FiscalPeriodId = fiscalPeriodId;
            spTransaction.FinancialAction = transTransactorDef.FinancialTransAction;

            // Apply financial action to derived amounts
            ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, spTransaction);

            await _context.SaveChangesAsync();

            // Remove existing mappings and CFA transactions for this doc
            var docId = spTransaction.Id;
            _context.BuyDocTransPaymentMappings.RemoveRange(
                _context.BuyDocTransPaymentMappings.Where(p => p.TransactorTransactionId == docId));
            _context.SellDocTransPaymentMappings.RemoveRange(
                _context.SellDocTransPaymentMappings.Where(p => p.TransactorTransactionId == docId));
            _context.CashFlowAccountTransactions.RemoveRange(
                _context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == oldTransCreatorSectionId && p.CreatorId == oldTransCreatorId));
            // Backup this logic on 2025/11/2 to check in the database if new logic above creates any issues
            // _context.CashFlowAccountTransactions.RemoveRange(
            //                 _context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == oldTransSectionId && p.CreatorId == docId));

            // Re-create CFA transaction if applicable
            if (itemVm.CfAccountId > 0)
            {
                var cfaSeriesId = docSeries.DefaultCfaTransSeriesId;
               
                if (cfaSeriesId > 0)
                {
                    //New Logic
                    var cfaTrandCreateDto = new CfaTransactionCreateDto()
                    {
                        TransDate = itemVm.TransDate,
                        CashFlowAccountId = itemVm.CfAccountId,
                        CompanyId = itemVm.CompanyId,
                        DocSeriesId = cfaSeriesId,
                        Etiology = itemVm.Etiology,
                        FiscalPeriodId = spTransaction.FiscalPeriodId,
                               
                        TransRefCode = spTransaction.TransRefCode,
                        Amount = itemVm.AmountSum,
                        CreatorSectionId = sectionId,
                        CreatorId = spTransaction.Id
                    
                    };
                
                    var cfaResult = await _cfaTransSrv.AddCFATransaction(cfaTrandCreateDto);
                    if (!cfaResult.Success)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        return ServiceResult.Error($"CashFlow Account Transaction failed to update", "BADREQUEST");
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}", "BADREQUEST");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return ServiceResult.Ok(spTransaction);
    }

    public async Task<ServiceResult> DeleteTransactorTransaction(int id)
    {
        if (id <= 0)
        {
            return ServiceResult.Error("No Document Id", "ARGUMENT_ERROR");
        }
        // Get the transaction (no tracking) to determine section for related deletions
        var spTrans = await _context.TransactorTransactions
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id);
        if (spTrans == null)
        {
            return ServiceResult.Error("Η συναλλαγή δεν βρέθηκε", "BADREQUEST");
        }

        int sectionId = spTrans.SectionId;
        if (sectionId == 0)
        {
            var sectn = await _context.Sections
                .SingleOrDefaultAsync(s => s.SystemName == GrKouk.Erp.Definitions.Constants.SectionTransactorTransactions);
            if (sectn == null)
            {
                return ServiceResult.Error("Δεν υπάρχει το Section", "BADREQUEST");
            }
            sectionId = sectn.Id;
        }

        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            // Remove related mappings
            _context.BuyDocTransPaymentMappings.RemoveRange(
                _context.BuyDocTransPaymentMappings.Where(p => p.TransactorTransactionId == spTrans.Id));
            _context.SellDocTransPaymentMappings.RemoveRange(
                _context.SellDocTransPaymentMappings.Where(p => p.TransactorTransactionId == spTrans.Id));

            // Remove related cash flow transactions created by this document
            _context.CashFlowAccountTransactions.RemoveRange(
                _context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == sectionId && p.CreatorId == spTrans.Id));

            // Remove the transaction entity itself (use tracked entity)
            var tracked = await _context.TransactorTransactions.FindAsync(spTrans.Id);
            if (tracked != null)
            {
                _context.TransactorTransactions.Remove(tracked);
            }

            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}", "BADREQUEST");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return ServiceResult.Ok();
    }
}