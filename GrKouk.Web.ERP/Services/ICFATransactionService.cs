using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface ICFATransactionService
{
    Task<ServiceResult> AddCFATransaction(CfaTransactionCreateDto itemVm,string  callerSectionCode=null);
    Task<ServiceResult> ModifyCFATransaction(CfaTransactionModifyDto itemVm, string  callerSectionCode=null);
    Task<ServiceResult> DeleteCFATransaction(int id);
}

public class CFATransactionService : ICFATransactionService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<CFATransactionService> _logger;
    private const string _defaultSectionCode = GrKouk.Erp.Definitions.Constants.SectionCashFlowAccountsTransactions;
    public CFATransactionService(ApiDbContext context, ILogger<CFATransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<ServiceResult> AddCFATransaction(CfaTransactionCreateDto itemVm, string callerSectionCode = null)
    {
        #region Fiscal Period

        var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, itemVm.TransDate);
        if (fiscalPeriod == null)
        {
            return ServiceResult.Error("No Fiscal Period covers Transaction Date", "BADREQUEST");
            
        }

        #endregion

        var spTransaction = new CashFlowAccountTransaction
        {
            TransDate = itemVm.TransDate,
            DocumentSeriesId = itemVm.DocSeriesId,
           
            CashFlowAccountId = itemVm.CashFlowAccountId,
            RefCode = itemVm.TransRefCode,
            CompanyId = itemVm.CompanyId,
            
            Amount = itemVm.Amount,
            Etiology = itemVm.Etiology,
           

        };
        var docSeries = await
            _context.CashFlowDocSeriesDefs.SingleOrDefaultAsync(m =>
                m.Id == itemVm.DocSeriesId);

        if (docSeries is null)
        {
            return ServiceResult.Error("Δεν βρέθηκε η σειρά του παραστατικού", "BADREQUEST");
        }
        await _context.Entry(docSeries).Reference(t => t.CashFlowDocTypeDefinition).LoadAsync();

        var docTypeDef = docSeries.CashFlowDocTypeDefinition;
        await _context.Entry(docTypeDef)
            .Reference(t => t.CashFlowTransactionDefinition)
            .LoadAsync();

        var transTransactorDef = docTypeDef.CashFlowTransactionDefinition;
        
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

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            spTransaction.SectionId = sectionId;
            spTransaction.DocumentTypeId = docSeries.CashFlowDocTypeDefId;
            spTransaction.FiscalPeriodId = fiscalPeriod.Id;
            spTransaction.CfaAction = transTransactorDef.CfaAction;
            ActionHandlers.CashFlowFinAction(transTransactorDef.CfaAction, spTransaction);
            await _context.CashFlowAccountTransactions.AddAsync(spTransaction);
            await _context.SaveChangesAsync();
            
                
            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred: {Error}", ex.Message);
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

    public async Task<ServiceResult> ModifyCFATransaction(CfaTransactionModifyDto itemVm, string callerSectionCode = null)
    {
         #region Fiscal Period

        var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, itemVm.TransDate);
        if (fiscalPeriod == null)
        {
            return ServiceResult.Error("No Fiscal Period covers Transaction Date", "BADREQUEST");
        }

        #endregion

        // Load doc series and definitions
        var docSeries = await _context.CashFlowDocSeriesDefs
            .SingleOrDefaultAsync(m => m.Id == itemVm.DocSeriesId);
        if (docSeries is null)
        {
            return ServiceResult.Error("Δεν βρέθηκε η σειρά παραστατικο", "BADREQUEST");
        }
        await _context.Entry(docSeries).Reference(t => t.CashFlowDocTypeDefinition).LoadAsync();
        var docTypeDef = docSeries.CashFlowDocTypeDefinition;
        await _context.Entry(docTypeDef).Reference(t => t.CashFlowTransactionDefinition).LoadAsync();
        var transTransactorDef = docTypeDef.CashFlowTransactionDefinition;

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
        // var spOldTrans = await _context.CashFlowAccountTransactions
        //     .AsNoTracking()
        //     .SingleOrDefaultAsync(p => p.Id == itemVm.Id);
        // if (spOldTrans == null)
        // {
        //     return ServiceResult.Error("Η συναλλαγή δεν βρέθηκε", "BADREQUEST");
        // }
        // int oldTransSectionId = spOldTrans.SectionId == 0 ? sectionId : spOldTrans.SectionId;

        var spTransaction = await _context.CashFlowAccountTransactions
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
            spTransaction.DocumentSeriesId = itemVm.DocSeriesId;
            spTransaction.DocumentTypeId = docSeries.CashFlowDocTypeDefId;
            spTransaction.CashFlowAccountId = itemVm.CashFlowAccountId;
            spTransaction.RefCode = itemVm.TransRefCode;
            spTransaction.CompanyId = itemVm.CompanyId;
            spTransaction.Amount = itemVm.Amount;
            spTransaction.Etiology = itemVm.Etiology;
            spTransaction.SectionId = sectionId;
            spTransaction.FiscalPeriodId = fiscalPeriod.Id;
            spTransaction.CfaAction = transTransactorDef.CfaAction;

            // Apply financial action to derived amounts
            ActionHandlers.CashFlowFinAction(transTransactorDef.CfaAction, spTransaction);

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

    public async Task<ServiceResult> DeleteCFATransaction(int id)
    {
        if (id <= 0)
        {
            return ServiceResult.Error("No Document Id", "ARGUMENT_ERROR");
        }
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            
            // Remove the transaction entity itself (use tracked entity)
            var itemToDelete = await _context.TransactorTransactions.FindAsync(id);
            if (itemToDelete != null)
            {
                _context.TransactorTransactions.Remove(itemToDelete);
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