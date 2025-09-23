using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IDocumentTransactionService
{
    Task<IActionResult> AddBuyDocument(BuyDocCreateAjaxDto docTrans);
    Task<IActionResult> ModifyBuyDocument(BuyDocModifyAjaxDto docTrans);
    Task<ServiceResult> DeleteBuyDocument(int docId);
    Task<IActionResult> AddSalesDoc(SellDocCreateAjaxDto docTrans);
    Task<IActionResult> ModifySalesDoc(SellDocModifyAjaxDto docTrans);
    Task<ServiceResult> DeleteSaleDocument(int docId);
}

public class DocumentTransactionService : IDocumentTransactionService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<DocumentTransactionService> _logger;
    private readonly IMapper _mapper;

    public DocumentTransactionService(ApiDbContext context, ILogger<DocumentTransactionService> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IActionResult> AddBuyDocument(BuyDocCreateAjaxDto docTrans)
    {
        _logger.LogInformation("AddBuyDocument");
        const string sectionCode = "SYS-BUY-MATERIALS-SCN";
        bool noWarehouseTrans = false;
        int newDocumentId = 0;

        BuyDocCreateAjaxNoLinesDto transToAttachNoLines;
        BuyDocument transToAttach;
        DateTime dateOfTrans;

        if (docTrans == null)
        {
            return new BadRequestObjectResult(new
            {
                error = "Empty request docTrans"
            });
        }

        try
        {
            transToAttachNoLines = _mapper.Map<BuyDocCreateAjaxNoLinesDto>(docTrans);
            transToAttach = _mapper.Map<BuyDocument>(transToAttachNoLines);
            dateOfTrans = docTrans.TransDate;
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new
            {
                error = e.Message
            });
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            #region Fiscal Period

            var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
            if (fiscalPeriod == null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "No Fiscal Period covers Transaction Date"
                });
            }

            #endregion

            var docSeries = await
                _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == docTrans.BuyDocSeriesId);

            if (docSeries is null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "Buy Doc Series not found"
                });
            }

            await _context.Entry(docSeries).Reference(t => t.BuyDocTypeDef).LoadAsync();
            var docTypeDef = docSeries.BuyDocTypeDef;

            await _context.Entry(docTypeDef)
                .Reference(t => t.TransTransactorDef)
                .LoadAsync();

            await _context.Entry(docTypeDef).Reference(t => t.TransWarehouseDef)
                .LoadAsync();

            #region Section Management

            int sectionId = 0;
            if (docTypeDef.SectionId == 0)
            {
                var sectn = await _context.Sections.AsNoTracking()
                    .SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                if (sectn == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Could not locate section "
                    });
                }

                sectionId = sectn.Id;
            }
            else
            {
                sectionId = docTypeDef.SectionId;
            }

            #endregion

            var transTransactorDef = docTypeDef.TransTransactorDef;
            var transWarehouseDef = docTypeDef.TransWarehouseDef;

            transToAttach.SectionId = sectionId;
            transToAttach.FiscalPeriodId = fiscalPeriod.Id;
            transToAttach.BuyDocTypeId = docSeries.BuyDocTypeDefId;
            await _context.BuyDocuments.AddAsync(transToAttach);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (ownsTransaction) await transaction.RollbackAsync();
                string msg = e.InnerException?.Message;
                return new BadRequestObjectResult(new
                {
                    error = e.Message + " " + msg
                });
            }

            var docId = _context.Entry(transToAttach).Entity.Id;
            newDocumentId = docId;

            if (transTransactorDef.DefaultDocSeriesId > 0)
            {
                var transTransactorDefaultSeries = await
                    _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                        p.Id == transTransactorDef.DefaultDocSeriesId);
                if (transTransactorDefaultSeries == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Default series for transactor transaction not found"
                    });
                }

                var sTransactorTransaction = _mapper.Map<TransactorTransaction>(docTrans);
                sTransactorTransaction.TransactorId = docTrans.TransactorId;
                sTransactorTransaction.SectionId = sectionId;
                sTransactorTransaction.CfAccountId = 0;
                sTransactorTransaction.CreatorSectionId = sectionId;
                sTransactorTransaction.TransTransactorDocTypeId =
                    transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                sTransactorTransaction.CreatorId = docId;
                ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, sTransactorTransaction);
                transToAttach.TransNetAmount = sTransactorTransaction.TransNetAmount;
                transToAttach.TransFpaAmount = sTransactorTransaction.TransFpaAmount;
                transToAttach.TransDiscountAmount = sTransactorTransaction.TransDiscountAmount;
                transToAttach.TransExpensesAmount = 0;
                _context.Entry(transToAttach).State = EntityState.Modified;
                await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                try
                {
                    await _context.SaveChangesAsync();
                    //throw new Exception("Test");;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }

            var paymentMethod =
                await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
            if (paymentMethod is null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                });
            }

            if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
            {
                var autoPaySeriesId = transToAttach.BuyDocSeries.PayoffSeriesId;
                var paymentCfAccountId = paymentMethod.CfAccountId;
                if (autoPaySeriesId > 0)
                {
                    var transTransactorPayOffSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == autoPaySeriesId);
                    if (transTransactorPayOffSeries == null)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "AutoPayOff series not found"
                        });
                    }

                    var transactor = await _context.Transactors
                        .Where(p => p.Id == docTrans.TransactorId)
                        .SingleOrDefaultAsync();
                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(docTrans);
                    var transTransactorEtiology =
                        $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";
                    sTransactorTransaction.TransactorId = docTrans.TransactorId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorPayOffSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.Etiology = transTransactorEtiology;
                    sTransactorTransaction.CreatorId = docId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    await _context.Entry(transTransactorPayOffSeries)
                        .Reference(t => t.TransTransactorDocTypeDef)
                        .LoadAsync();
                    var transTransactorDocTypeDef = transTransactorPayOffSeries.TransTransactorDocTypeDef;

                    #region Section Management

                    if (transTransactorDocTypeDef.SectionId == 0)
                    {
                        sTransactorTransaction.SectionId = sectionId;
                    }
                    else
                    {
                        sTransactorTransaction.SectionId = transTransactorDocTypeDef.SectionId;
                    }

                    #endregion

                    await _context.Entry(transTransactorDocTypeDef)
                        .Reference(t => t.TransTransactorDef)
                        .LoadAsync();
                    var transPaymentTransactorDef = transTransactorDocTypeDef.TransTransactorDef;

                    ActionHandlers.TransactorFinAction(transPaymentTransactorDef.FinancialTransAction,
                        sTransactorTransaction);
                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    try
                    {
                        await _context.SaveChangesAsync();
                        // throw new Exception("Test");;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }


                    if (paymentCfAccountId > 0)
                    {
                        var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                        if (defaultCfaSeriesId > 0)
                        {
                            var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                            if (cfaSeries != null)
                            {
                                await _context.Entry(cfaSeries)
                                    .Reference(t => t.CashFlowDocTypeDefinition)
                                    .LoadAsync();

                                var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                if (cfaType != null)
                                {
                                    await _context.Entry(cfaType)
                                        .Reference(t => t.CashFlowTransactionDefinition)
                                        .LoadAsync();

                                    var etiology =
                                        $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";

                                    var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                    var cfaTrans = new CashFlowAccountTransaction
                                    {
                                        TransDate = docTrans.TransDate,
                                        CashFlowAccountId = paymentCfAccountId,
                                        CompanyId = docTrans.CompanyId,
                                        DocumentSeriesId = cfaSeries.Id,
                                        DocumentTypeId = cfaType.Id,
                                        Etiology = etiology,
                                        FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                        CreatorSectionId = sectionId,
                                        CreatorId = docId,
                                        RefCode = docTrans.TransRefCode,
                                        Amount = sTransactorTransaction.AmountNet -
                                                 sTransactorTransaction.AmountDiscount +
                                                 sTransactorTransaction.AmountFpa,
                                        SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                    };
                                    ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                    await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                    sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                    _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        //throw new Exception("Test");;
                                    }
                                    catch (Exception e)
                                    {
                                        if (ownsTransaction) await transaction.RollbackAsync();
                                        string msg = e.InnerException?.Message;
                                        return new BadRequestObjectResult(new
                                        {
                                            error = e.Message + " " + msg
                                        });
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                        var payOffMapping = new BuyDocTransPaymentMapping()
                        {
                            BuyDocumentId = docId,
                            TransactorTransactionId = payOfTransactionId,
                            AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                         sTransactorTransaction.AmountDiscount
                        };
                        await _context.BuyDocTransPaymentMappings.AddAsync(payOffMapping);
                    }
                    catch (Exception e)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }
                }
            }

            int warehouseSeriesId = 0;
            int warehouseTypeId = 0;

            if (transWarehouseDef.DefaultDocSeriesId > 0)
            {
                var transWarehouseDefaultSeries =
                    await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                        p.Id == transWarehouseDef.DefaultDocSeriesId);
                if (transWarehouseDefaultSeries == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Default series for warehouse transaction not found"
                    });
                }

                noWarehouseTrans = false;
                warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
            }
            else
            {
                noWarehouseTrans = true;
            }

            foreach (var dataBuyDocLine in docTrans.BuyDocLines)
            {
                var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                var material = await _context.WarehouseItems
                    .SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                if (material is null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Could not locate material in Doc Line "
                    });
                }

                #region MaterialLine

                var transUnitId = dataBuyDocLine.TransactionUnitId;
                var transUnitFactor = dataBuyDocLine.TransactionUnitFactor;
                decimal transPrice = dataBuyDocLine.TransUnitPrice;
                double transUnits = dataBuyDocLine.TransactionQuantity;
                decimal units = (decimal)dataBuyDocLine.Q1;
                decimal unitPrice = dataBuyDocLine.Price;
                decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                decimal lineNetAmount = unitPrice * units;
                decimal lineDiscountAmount = lineNetAmount * discountRate;
                decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                var lineAmounts = new DocLineFinancialActionAmounts
                {
                    AmountNet = lineNetAmount,
                    AmountFpa = lineFpaAmount,
                    AmountDiscount = lineDiscountAmount,
                    AmountExpenses = 0
                };
                ActionHandlers.DocLineFinAction(transTransactorDef.FinancialTransAction, lineAmounts);
                var buyMaterialLine = new BuyDocLine
                {
                    UnitPrice = unitPrice,
                    AmountFpa = lineFpaAmount,
                    AmountNet = lineNetAmount,
                    AmountDiscount = lineDiscountAmount,
                    DiscountRate = discountRate,
                    FpaRate = fpaRate,
                    WarehouseItemId = dataBuyDocLine.WarehouseItemId,
                    Quontity1 = dataBuyDocLine.Q1,
                    Quontity2 = dataBuyDocLine.Q2,
                    PrimaryUnitId = dataBuyDocLine.MainUnitId,
                    SecondaryUnitId = dataBuyDocLine.SecUnitId,
                    Factor = dataBuyDocLine.Factor,
                    BuyDocumentId = docId,
                    Etiology = transToAttach.Etiology,
                    TransactionUnitId = transUnitId,
                    TransactionQuantity = transUnits,
                    TransUnitPrice = transPrice,
                    TransactionUnitFactor = transUnitFactor,
                    TransNetAmount = lineAmounts.TransNetAmount,
                    TransFpaAmount = lineAmounts.TransFpaAmount,
                    TransDiscountAmount = lineAmounts.TransDiscountAmount,
                    TransExpensesAmount = lineAmounts.TransExpensesAmount
                };
                transToAttach.BuyDocLines.Add(buyMaterialLine);

                #endregion

                if (!noWarehouseTrans)
                {
                    #region Warehouse transaction

                    var warehouseTrans = new WarehouseTransaction
                    {
                        FpaRate = fpaRate,
                        DiscountRate = discountRate,
                        UnitPrice = unitPrice,
                        AmountDiscount = lineDiscountAmount,
                        AmountNet = lineNetAmount,
                        AmountFpa = lineFpaAmount,
                        CompanyId = transToAttach.CompanyId,
                        Etiology = transToAttach.Etiology,
                        FiscalPeriodId = transToAttach.FiscalPeriodId,
                        WarehouseItemId = warehouseItemId,
                        PrimaryUnitId = dataBuyDocLine.MainUnitId,
                        SecondaryUnitId = dataBuyDocLine.SecUnitId,
                        SectionId = sectionId,
                        CreatorId = transToAttach.Id,
                        TransDate = transToAttach.TransDate,
                        TransRefCode = transToAttach.TransRefCode,
                        UnitFactor = (decimal)dataBuyDocLine.Factor,
                        TransWarehouseDocSeriesId = warehouseSeriesId,
                        TransWarehouseDocTypeId = warehouseTypeId
                    };

                    ActionHandlers.ItemNatureHandler(material.WarehouseItemNature, warehouseTrans,
                        transWarehouseDef);
                    ActionHandlers.ItemInventoryActionHandler(warehouseTrans.InventoryAction, dataBuyDocLine.Q1,
                        dataBuyDocLine.Q2,
                        warehouseTrans);
                    ActionHandlers.ItemInventoryValueActionHandler(warehouseTrans.InventoryValueAction,
                        warehouseTrans);
                    await _context.WarehouseTransactions.AddAsync(warehouseTrans);

                    #endregion
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                if (ownsTransaction) await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (ownsTransaction) await transaction.RollbackAsync();
                string msg = e.InnerException?.Message;
                return new BadRequestObjectResult(new
                {
                    error = e.Message + " " + msg
                });
            }
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return new OkObjectResult(newDocumentId);
    }

    public async Task<IActionResult> ModifyBuyDocument(BuyDocModifyAjaxDto docTrans)
    {
        const string sectionCode = "SYS-BUY-MATERIALS-SCN";
        bool noWarehouseTrans;

        BuyDocModifyAjaxNoLinesDto transToAttachNoLines;
        BuyDocument transToAttach;
        DateTime dateOfTrans;
        if (docTrans == null)
        {
            return new BadRequestObjectResult(new
            {
                error = "Empty request docTrans"
            });
        }

        try
        {
            transToAttachNoLines = _mapper.Map<BuyDocModifyAjaxNoLinesDto>(docTrans);
            transToAttach = _mapper.Map<BuyDocument>(transToAttachNoLines);
            dateOfTrans = docTrans.TransDate;
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new
            {
                error = e.Message
            });
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            _context.BuyDocLines.RemoveRange(_context.BuyDocLines.Where(p => p.BuyDocumentId == docTrans.Id));
            _context.TransactorTransactions.RemoveRange(
                _context.TransactorTransactions.Where(p =>
                    p.CreatorSectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));
            _context.WarehouseTransactions.RemoveRange(
                _context.WarehouseTransactions.Where(p =>
                    p.SectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));
            _context.BuyDocTransPaymentMappings.RemoveRange(
                _context.BuyDocTransPaymentMappings.Where(p => p.BuyDocumentId == docTrans.Id));
            _context.CashFlowAccountTransactions.RemoveRange(
                _context.CashFlowAccountTransactions.Where(p =>
                    p.CreatorSectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));

            #region Fiscal Period

            var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
            if (fiscalPeriod == null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "No Fiscal Period covers Transaction Date"
                });
            }

            #endregion

            var docSeries = await
                _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == docTrans.BuyDocSeriesId);

            if (docSeries is null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "Buy Doc Series not found"
                });
            }

            await _context.Entry(docSeries).Reference(t => t.BuyDocTypeDef).LoadAsync();
            var docTypeDef = docSeries.BuyDocTypeDef;

            await _context.Entry(docTypeDef).Reference(t => t.TransTransactorDef)
                .LoadAsync();
            await _context.Entry(docTypeDef).Reference(t => t.TransWarehouseDef)
                .LoadAsync();

            #region Section Management

            int sectionId = 0;
            if (docTypeDef.SectionId == 0)
            {
                var sectn = await _context.Sections.AsNoTracking()
                    .SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                if (sectn == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Could not locate section "
                    });
                }

                sectionId = sectn.Id;
            }
            else
            {
                sectionId = docTypeDef.SectionId;
            }

            #endregion

            var transTransactorDef = docTypeDef.TransTransactorDef;
            //var transSupplierDef = docTypeDef.TransSupplierDef;
            var transWarehouseDef = docTypeDef.TransWarehouseDef;

            transToAttach.SectionId = sectionId;
            transToAttach.FiscalPeriodId = fiscalPeriod.Id;
            transToAttach.BuyDocTypeId = docSeries.BuyDocTypeDefId;

            _context.Entry(transToAttach).State = EntityState.Modified;
            var docId = transToAttach.Id;
            //--------------------------------------
            if (transTransactorDef.DefaultDocSeriesId > 0)
            {
                var transTransactorDefaultSeries = await
                    _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                        p.Id == transTransactorDef.DefaultDocSeriesId);
                if (transTransactorDefaultSeries == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Default series for transactor transaction not found"
                    });
                }

                try
                {
                    var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(docTrans);
                    //Ετσι δεν μεταφέρει το Id απο το docTrans
                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);

                    sTransactorTransaction.TransactorId = docTrans.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction,
                        sTransactorTransaction);
                    // Update document transaction with transamounts
                    transToAttach.TransNetAmount = sTransactorTransaction.TransNetAmount;
                    transToAttach.TransFpaAmount = sTransactorTransaction.TransFpaAmount;
                    transToAttach.TransDiscountAmount = sTransactorTransaction.TransDiscountAmount;
                    transToAttach.TransExpensesAmount = 0;
                    _context.Entry(transToAttach).State = EntityState.Modified;
                    //----------------------------------------------
                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                }
                catch (Exception e)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }

            //Αυτόματη εξόφληση
            var paymentMethod =
                await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
            if (paymentMethod is null)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return new NotFoundObjectResult(new
                {
                    error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                });
            }

            if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
            {
                var autoPaySeriesId = transToAttach.BuyDocSeries.PayoffSeriesId;
                var paymentCfAccountId = paymentMethod.CfAccountId;
                if (autoPaySeriesId > 0)
                {
                    var transTransactorPayOffSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == autoPaySeriesId);
                    if (transTransactorPayOffSeries == null)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "AutoPayOff series not found"
                        });
                    }

                    var transactor = await _context.Transactors
                        .Where(p => p.Id == docTrans.TransactorId)
                        .SingleOrDefaultAsync();
                    var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(docTrans);
                    //Ετσι δεν μεταφέρει το Id απο το docTrans
                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);
                    var transTransactorEtiology =
                        $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";
                    sTransactorTransaction.TransactorId = docTrans.TransactorId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorPayOffSeries.TransTransactorDocTypeDefId;

                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.Etiology = transTransactorEtiology;
                    sTransactorTransaction.CreatorId = docId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    await _context.Entry(transTransactorPayOffSeries)
                        .Reference(t => t.TransTransactorDocTypeDef)
                        .LoadAsync();
                    var transTransactorDocTypeDef = transTransactorPayOffSeries.TransTransactorDocTypeDef;

                    #region Section Management

                    if (transTransactorDocTypeDef.SectionId == 0)
                    {
                        sTransactorTransaction.SectionId = sectionId;
                    }
                    else
                    {
                        sTransactorTransaction.SectionId = transTransactorDocTypeDef.SectionId;
                    }

                    #endregion

                    await _context.Entry(transTransactorDocTypeDef)
                        .Reference(t => t.TransTransactorDef)
                        .LoadAsync();
                    var transPaymentTransactorDef = transTransactorDocTypeDef.TransTransactorDef;
                    ActionHandlers.TransactorFinAction(transPaymentTransactorDef.FinancialTransAction,
                        sTransactorTransaction);

                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }

                    //Cash Flow Account Transaction 
                    if (paymentCfAccountId > 0)
                    {
                        var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                        if (defaultCfaSeriesId > 0)
                        {
                            var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                            if (cfaSeries != null)
                            {
                                await _context.Entry(cfaSeries)
                                    .Reference(t => t.CashFlowDocTypeDefinition)
                                    .LoadAsync();

                                var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                if (cfaType != null)
                                {
                                    await _context.Entry(cfaType)
                                        .Reference(t => t.CashFlowTransactionDefinition)
                                        .LoadAsync();

                                    var etiology =
                                        $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";


                                    var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                    var cfaTrans = new CashFlowAccountTransaction
                                    {
                                        TransDate = docTrans.TransDate,
                                        CashFlowAccountId = paymentCfAccountId,
                                        CompanyId = docTrans.CompanyId,
                                        DocumentSeriesId = cfaSeries.Id,
                                        DocumentTypeId = cfaType.Id,
                                        Etiology = etiology,
                                        FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                        CreatorSectionId = sectionId,
                                        CreatorId = docId,
                                        RefCode = docTrans.TransRefCode,
                                        Amount = sTransactorTransaction.AmountNet -
                                                 sTransactorTransaction.AmountDiscount +
                                                 sTransactorTransaction.AmountFpa,
                                        SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                    };
                                    ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                    await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                    sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                    _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                    }
                                    catch (Exception e)
                                    {
                                        if (ownsTransaction) await transaction.RollbackAsync();
                                        string msg = e.InnerException?.Message;
                                        return new BadRequestObjectResult(new
                                        {
                                            error = e.Message + " " + msg
                                        });
                                    }
                                }
                            }
                        }
                    }

                    //End Cash Flow Account Transaction 
                    try
                    {
                        var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                        var payOffMapping = new BuyDocTransPaymentMapping()
                        {
                            BuyDocumentId = docId,
                            TransactorTransactionId = payOfTransactionId,
                            AmountUsed = sTransactorTransaction.TransNetAmount +
                                         sTransactorTransaction.TransFpaAmount -
                                         sTransactorTransaction.TransDiscountAmount
                        };
                        await _context.BuyDocTransPaymentMappings.AddAsync(payOffMapping);
                    }
                    catch (Exception e)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }
                }


               
            }
            int warehouseSeriesId = 0;
            int warehouseTypeId = 0;

            if (transWarehouseDef.DefaultDocSeriesId > 0)
            {
                var transWarehouseDefaultSeries =
                    await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                        p.Id == transWarehouseDef.DefaultDocSeriesId);
                if (transWarehouseDefaultSeries == null)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Default series for warehouse transaction not found"
                    });
                }

                noWarehouseTrans = false;
                warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
            }
            else
            {
                noWarehouseTrans = true;
            }

            foreach (var dataBuyDocLine in docTrans.BuyDocLines)
            {
                var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                if (material is null)
                {
                    //Handle error
                    if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Could not locate material in Doc Line "
                    });
                }

                #region MaterialLine

                var transUnitId = dataBuyDocLine.TransactionUnitId;
                var transUnitFactor = dataBuyDocLine.TransactionUnitFactor;
                decimal transPrice = dataBuyDocLine.TransUnitPrice;
                double transUnits = dataBuyDocLine.TransactionQuantity;
                decimal units = (decimal)dataBuyDocLine.Q1;
                decimal unitPrice = dataBuyDocLine.Price;
                decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                decimal lineNetAmount = unitPrice * units;
                decimal lineDiscountAmount = lineNetAmount * discountRate;
                decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                var lineAmounts = new DocLineFinancialActionAmounts
                {
                    AmountNet = lineNetAmount,
                    AmountFpa = lineFpaAmount,
                    AmountDiscount = lineDiscountAmount,
                    AmountExpenses = 0
                };
                ActionHandlers.DocLineFinAction(transTransactorDef.FinancialTransAction, lineAmounts);
                var warehouseItemLine = new BuyDocLine
                {
                    UnitPrice = unitPrice,
                    AmountFpa = lineFpaAmount,
                    AmountNet = lineNetAmount,
                    AmountDiscount = lineDiscountAmount,
                    DiscountRate = discountRate,
                    FpaRate = fpaRate,
                    WarehouseItemId = dataBuyDocLine.WarehouseItemId,
                    Quontity1 = dataBuyDocLine.Q1,
                    Quontity2 = dataBuyDocLine.Q2,
                    PrimaryUnitId = dataBuyDocLine.MainUnitId,
                    SecondaryUnitId = dataBuyDocLine.SecUnitId,
                    Factor = dataBuyDocLine.Factor,
                    BuyDocumentId = docId,
                    Etiology = transToAttach.Etiology,
                    TransactionUnitId = transUnitId,
                    TransactionQuantity = transUnits,
                    TransUnitPrice = transPrice,
                    TransactionUnitFactor = transUnitFactor,
                    TransNetAmount = lineAmounts.TransNetAmount,
                    TransFpaAmount = lineAmounts.TransFpaAmount,
                    TransDiscountAmount = lineAmounts.TransDiscountAmount,
                    TransExpensesAmount = lineAmounts.TransExpensesAmount
                };

                //_context.Entry(transToAttach).Entity

                try
                {
                    transToAttach.BuyDocLines.Add(warehouseItemLine);
                }
                catch (Exception e)
                {
                    if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                #endregion

                if (!noWarehouseTrans)
                {
                    #region Warehouse transaction

                    var warehouseTrans = new WarehouseTransaction
                    {
                        FpaRate = fpaRate,
                        DiscountRate = discountRate,
                        UnitPrice = unitPrice,
                        AmountDiscount = lineDiscountAmount,
                        AmountNet = lineNetAmount,
                        AmountFpa = lineFpaAmount,
                        CompanyId = transToAttach.CompanyId,
                        Etiology = transToAttach.Etiology,
                        FiscalPeriodId = transToAttach.FiscalPeriodId,
                        WarehouseItemId = warehouseItemId,
                        PrimaryUnitId = dataBuyDocLine.MainUnitId,
                        SecondaryUnitId = dataBuyDocLine.SecUnitId,
                        SectionId = sectionId,
                        CreatorId = transToAttach.Id,
                        TransDate = transToAttach.TransDate,
                        TransRefCode = transToAttach.TransRefCode,
                        UnitFactor = (decimal)dataBuyDocLine.Factor,
                        TransWarehouseDocSeriesId = warehouseSeriesId,
                        TransWarehouseDocTypeId = warehouseTypeId
                    };

                    ActionHandlers.ItemNatureHandler(material.WarehouseItemNature, warehouseTrans,
                        transWarehouseDef);
                    ActionHandlers.ItemInventoryActionHandler(warehouseTrans.InventoryAction, dataBuyDocLine.Q1,
                        dataBuyDocLine.Q2,
                        warehouseTrans);
                    ActionHandlers.ItemInventoryValueActionHandler(warehouseTrans.InventoryValueAction,
                        warehouseTrans);

                    try
                    {
                        await _context.WarehouseTransactions.AddAsync(warehouseTrans);
                    }
                    catch (Exception e)
                    {
                        if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }

                    #endregion
                }
            }


            try
            {
                await _context.SaveChangesAsync();
                if (ownsTransaction) await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                string msg = e.InnerException?.Message;
                return new BadRequestObjectResult(new
                {
                    error = e.Message + " " + msg
                });
            }
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return new OkObjectResult(new { });
    }

    public async Task<ServiceResult> DeleteBuyDocument(int docId)
    {
        if (docId < 0)
        {
            return ServiceResult.Error("No Document Id", "ARGUMENT_ERROR");
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;


        var buyDocument = await _context.BuyDocuments.FindAsync(docId);
        if (buyDocument == null)
        {
            return ServiceResult.Error("No Document Found", "NOT_FOUND");
        }
        // bool generateTestError = buyDocument.TransRefCode == "7752";

        try
        {
            _context.BuyDocLines.RemoveRange(_context.BuyDocLines.Where(p => p.BuyDocumentId == docId));
            _context.TransactorTransactions.RemoveRange(_context.TransactorTransactions.Where(p =>
                p.CreatorSectionId == buyDocument.SectionId && p.CreatorId == docId));
            _context.CashFlowAccountTransactions.RemoveRange(
                _context.CashFlowAccountTransactions.Where(p =>
                    p.CreatorSectionId == buyDocument.SectionId && p.CreatorId == docId));
            _context.WarehouseTransactions.RemoveRange(
                _context.WarehouseTransactions.Where(p =>
                    p.SectionId == buyDocument.SectionId && p.CreatorId == docId));
            _context.BuyDocTransPaymentMappings.RemoveRange(
                _context.BuyDocTransPaymentMappings.Where(p => p.BuyDocumentId == docId));
            var syncDoc = await _context.SyncBuyDocuments.SingleOrDefaultAsync(p => p.ErpId == buyDocument.Id);
            if (syncDoc is not null)
            {
                var syncLog = await _context.SynchronizationLogs.SingleOrDefaultAsync(p => p.EntityId == syncDoc.Id);
                if (syncLog is not null)
                {
                    _context.SynchronizationLogs.Remove(syncLog);
                }

                _context.SyncBuyDocuments.Remove(syncDoc);
            }

            _context.BuyDocuments.Remove(buyDocument);

            await _context.SaveChangesAsync();
            // if (generateTestError)
            // {
            //     throw new Exception("Test Error");
            // }
            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
            return ServiceResult.Error(msg, "ERROR");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return ServiceResult.Ok();
    }

    public async Task<IActionResult> AddSalesDoc(SellDocCreateAjaxDto docTrans)
    {
        _logger.LogInformation("AddSalesDocument");
         const string sectionCode = "SYS-SELL-COMBINED-SCN";
         
            bool noWarehouseTrans = false;
            int docId = 0;
            SellDocCreateAjaxNoLinesDto transToAttachNoLines;
            SellDocument transToAttach;
            DateTime dateOfTrans;

            if (docTrans == null)
            {
                return new BadRequestObjectResult(new
                {
                    error = "Empty request docTrans"
                });
            }

            try
            {
                transToAttachNoLines = _mapper.Map<SellDocCreateAjaxNoLinesDto>(docTrans);
                transToAttach = _mapper.Map<SellDocument>(transToAttachNoLines);
                dateOfTrans = docTrans.TransDate;
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                {
                    error = e.Message
                });
            }
            // Check if a transaction is already active
            bool ownsTransaction = _context.Database.CurrentTransaction == null;
            var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync()
                : _context.Database.CurrentTransaction;
            try
            {

                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == docTrans.SellDocSeriesId);

                if (docSeries is null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Buy Doc Series not found"
                    });
                }

                await _context.Entry(docSeries).Reference(t => t.SellDocTypeDef).LoadAsync();
                var docTypeDef = docSeries.SellDocTypeDef;

                await _context.Entry(docTypeDef)
                    .Reference(t => t.TransTransactorDef)
                    .LoadAsync();

                await _context.Entry(docTypeDef).Reference(t => t.TransWarehouseDef)
                    .LoadAsync();

                #region Section Management

                int sectionId = 0;
                if (docTypeDef.SectionId == 0)
                {
                    var sectn = await _context.Sections.AsNoTracking()
                        .SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else
                {
                    sectionId = docTypeDef.SectionId;
                }

                #endregion

                var transTransactorDef = docTypeDef.TransTransactorDef;
                var transWarehouseDef = docTypeDef.TransWarehouseDef;

                transToAttach.SectionId = sectionId;
                transToAttach.FiscalPeriodId = fiscalPeriod.Id;
                transToAttach.SellDocTypeId = docSeries.SellDocTypeDefId;
                await _context.SellDocuments.AddAsync(transToAttach);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                     if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                docId = _context.Entry(transToAttach).Entity.Id;


                if (transTransactorDef.DefaultDocSeriesId > 0)
                {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(docTrans);
                    sTransactorTransaction.TransactorId = docTrans.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction,
                        sTransactorTransaction);
                    // Update document transaction with transamounts
                    transToAttach.TransNetAmount = sTransactorTransaction.TransNetAmount;
                    transToAttach.TransFpaAmount = sTransactorTransaction.TransFpaAmount;
                    transToAttach.TransDiscountAmount = sTransactorTransaction.TransDiscountAmount;
                    transToAttach.TransExpensesAmount = 0;
                    _context.Entry(transToAttach).State = EntityState.Modified;
                    //----------------------------------------------
                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return new BadRequestObjectResult(new
                        {
                            error = e.Message + " " + msg
                        });
                    }
                }

                #region Αυτόματη Εξόφληση

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
                {
                    var autoPaySeriesId = transToAttach.SellDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0)
                    {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            return new NotFoundObjectResult(new
                            {
                                error = "AutoPayOff series not found"
                            });
                        }

                        var transactor = await _context.Transactors
                            .Where(p => p.Id == docTrans.TransactorId)
                            .SingleOrDefaultAsync();
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(docTrans);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";

                        sTransactorTransaction.TransactorId = docTrans.TransactorId;

                        sTransactorTransaction.TransTransactorDocTypeId =
                            transTransactorPayOffSeries.TransTransactorDocTypeDefId;
                        sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                        sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                        sTransactorTransaction.Etiology = transTransactorEtiology;
                        sTransactorTransaction.CreatorId = docId;
                        sTransactorTransaction.CreatorSectionId = sectionId;
                        await _context.Entry(transTransactorPayOffSeries)
                            .Reference(t => t.TransTransactorDocTypeDef)
                            .LoadAsync();
                        var transTransactorDocTypeDef = transTransactorPayOffSeries.TransTransactorDocTypeDef;

                        #region Section Management

                        if (transTransactorDocTypeDef.SectionId == 0)
                        {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else
                        {
                            sTransactorTransaction.SectionId = transTransactorDocTypeDef.SectionId;
                        }

                        #endregion

                        await _context.Entry(transTransactorDocTypeDef)
                            .Reference(t => t.TransTransactorDef)
                            .LoadAsync();
                        var transPaymentTransactorDef = transTransactorDocTypeDef.TransTransactorDef;
                        ActionHandlers.TransactorFinAction(transPaymentTransactorDef.FinancialTransAction,
                            sTransactorTransaction);

                        await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return new BadRequestObjectResult(new
                            {
                                error = e.Message + " " + msg
                            });
                        }

                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0)
                        {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0)
                            {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null)
                                {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null)
                                    {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";


                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction
                                        {
                                            TransDate = docTrans.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = docTrans.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = docTrans.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet -
                                                     sTransactorTransaction.AmountDiscount +
                                                     sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e)
                                        {
                                             if (ownsTransaction) await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return new BadRequestObjectResult(new
                                            {
                                                error = e.Message + " " + msg
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try
                        {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new SellDocTransPaymentMapping()
                            {
                                SellDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                             sTransactorTransaction.AmountDiscount
                            };
                            await _context.SellDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return new BadRequestObjectResult(new
                            {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }

                #endregion

                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0)
                {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs
                            .FirstOrDefaultAsync(p =>
                                p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else
                {
                    noWarehouseTrans = true;
                }

                foreach (var docLine in docTrans.SellDocLines)
                {
                    var warehouseItemId = docLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null)
                    {
                        //Handle error
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    var transUnitId = docLine.TransactionUnitId;
                    var transUnitFactor = docLine.TransactionUnitFactor;
                    // var factor = dataBuyDocLine.Factor;
                    decimal transPrice = docLine.TransUnitPrice;
                    double transUnits = docLine.TransactionQuantity;
                    decimal unitPrice = docLine.Price;
                    decimal units = (decimal)docLine.Q1;
                    decimal fpaRate = (decimal)docLine.FpaRate;
                    decimal discountRate = (decimal)docLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    var lineAmounts = new DocLineFinancialActionAmounts
                    {
                        AmountNet = lineNetAmount,
                        AmountFpa = lineFpaAmount,
                        AmountDiscount = lineDiscountAmount,
                        AmountExpenses = 0
                    };
                    ActionHandlers.DocLineFinAction(transTransactorDef.FinancialTransAction, lineAmounts);

                    var sellDocLine = new SellDocLine
                    {
                        UnitPrice = unitPrice,
                        AmountFpa = lineFpaAmount,
                        AmountNet = lineNetAmount,
                        AmountDiscount = lineDiscountAmount,
                        DiscountRate = discountRate,
                        FpaRate = fpaRate,
                        WarehouseItemId = docLine.WarehouseItemId,
                        Quontity1 = docLine.Q1,
                        Quontity2 = docLine.Q2,
                        PrimaryUnitId = docLine.MainUnitId,
                        SecondaryUnitId = docLine.SecUnitId,
                        Factor = docLine.Factor,
                        SellDocumentId = docId,
                        Etiology = transToAttach.Etiology,
                        TransactionUnitId = transUnitId,
                        TransactionQuantity = transUnits,
                        TransUnitPrice = transPrice,
                        TransactionUnitFactor = transUnitFactor,
                        TransNetAmount = lineAmounts.TransNetAmount,
                        TransFpaAmount = lineAmounts.TransFpaAmount,
                        TransDiscountAmount = lineAmounts.TransDiscountAmount,
                        TransExpensesAmount = lineAmounts.TransExpensesAmount
                    };


                    //_context.Entry(transToAttach).Entity
                    transToAttach.SellDocLines.Add(sellDocLine);

                    #endregion

                    if (!noWarehouseTrans)
                    {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction
                        {
                            FpaRate = fpaRate,
                            DiscountRate = discountRate,
                            UnitPrice = unitPrice,
                            AmountDiscount = lineDiscountAmount,
                            AmountNet = lineNetAmount,
                            AmountFpa = lineFpaAmount,
                            CompanyId = transToAttach.CompanyId,
                            Etiology = transToAttach.Etiology,
                            FiscalPeriodId = transToAttach.FiscalPeriodId,
                            WarehouseItemId = warehouseItemId,
                            PrimaryUnitId = docLine.MainUnitId,
                            SecondaryUnitId = docLine.SecUnitId,
                            SectionId = sectionId,
                            CreatorId = transToAttach.Id,
                            TransDate = transToAttach.TransDate,
                            TransRefCode = transToAttach.TransRefCode,
                            UnitFactor = (decimal)docLine.Factor,
                            TransWarehouseDocSeriesId = warehouseSeriesId,
                            TransWarehouseDocTypeId = warehouseTypeId
                        };


                        ActionHandlers.ItemNatureHandler(material.WarehouseItemNature, warehouseTrans,
                            transWarehouseDef);
                        ActionHandlers.ItemInventoryActionHandler(warehouseTrans.InventoryAction, docLine.Q1,
                            docLine.Q2,
                            warehouseTrans);
                        ActionHandlers.ItemInventoryValueActionHandler(warehouseTrans.InventoryValueAction,
                            warehouseTrans);

                        await _context.WarehouseTransactions.AddAsync(warehouseTrans);

                        #endregion
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                     if (ownsTransaction) await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                // Dispose the transaction only if we created it
                if (ownsTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
            // return Ok(new { });
            return new OkObjectResult(docId);
    }

    public async Task<IActionResult> ModifySalesDoc(SellDocModifyAjaxDto docTrans)
    {
         const string sectionCode = "SYS-SELL-COMBINED-SCN";
            bool noWarehouseTrans;

            SellDocModifyAjaxNoLinesDto transToAttachNoLines;
            SellDocument transToAttach;
            DateTime dateOfTrans;
            if (docTrans == null)
            {
                return new BadRequestObjectResult(new
                {
                    error = "Empty request docTrans"
                });
            }

            try
            {
                transToAttachNoLines = _mapper.Map<SellDocModifyAjaxNoLinesDto>(docTrans);
                transToAttach = _mapper.Map<SellDocument>(transToAttachNoLines);
                dateOfTrans = docTrans.TransDate;
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new
                {
                    error = e.Message
                });
            }
            // Check if a transaction is already active
            bool ownsTransaction = _context.Database.CurrentTransaction == null;
            var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync()
                : _context.Database.CurrentTransaction;
            try
            {

                _context.SellDocLines.RemoveRange(_context.SellDocLines.Where(p => p.SellDocumentId == docTrans.Id));
                _context.TransactorTransactions.RemoveRange(
                    _context.TransactorTransactions.Where(p =>
                        p.CreatorSectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));
                _context.WarehouseTransactions.RemoveRange(
                    _context.WarehouseTransactions.Where(p => p.SectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));
                _context.SellDocTransPaymentMappings.RemoveRange(
                    _context.SellDocTransPaymentMappings.Where(p => p.SellDocumentId == docTrans.Id));
                _context.CashFlowAccountTransactions.RemoveRange(
                    _context.CashFlowAccountTransactions.Where(p =>
                        p.CreatorSectionId == docTrans.SectionId && p.CreatorId == docTrans.Id));

                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == docTrans.SellDocSeriesId);

                if (docSeries is null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Buy Doc Series not found"
                    });
                }

                await _context.Entry(docSeries).Reference(t => t.SellDocTypeDef).LoadAsync();
                var docTypeDef = docSeries.SellDocTypeDef;
                await _context.Entry(docTypeDef)
                    .Reference(t => t.TransTransactorDef)
                    .LoadAsync();
                await _context.Entry(docTypeDef).Reference(t => t.TransTransactorDef)
                    .LoadAsync();
                await _context.Entry(docTypeDef).Reference(t => t.TransWarehouseDef)
                    .LoadAsync();

                #region Section Management

                int sectionId = 0;
                if (docTypeDef.SectionId == 0)
                {
                    var sectn = await _context.Sections.AsNoTracking()
                        .SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else
                {
                    sectionId = docTypeDef.SectionId;
                }

                #endregion

                var transTransactorDef = docTypeDef.TransTransactorDef;
                // var transSupplierDef = docTypeDef.TransSupplierDef;
                var transWarehouseDef = docTypeDef.TransWarehouseDef;

                transToAttach.SectionId = sectionId;
                transToAttach.FiscalPeriodId = fiscalPeriod.Id;
                transToAttach.SellDocTypeId = docSeries.SellDocTypeDefId;

                _context.Entry(transToAttach).State = EntityState.Modified;
                var docId = transToAttach.Id;
                //--------------------------------------
                if (transTransactorDef.DefaultDocSeriesId > 0)
                {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(docTrans);
                    //Ετσι δεν μεταφέρει το Id απο το docTrans
                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);

                    sTransactorTransaction.TransactorId = docTrans.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction,
                        sTransactorTransaction);
                    // Update document transaction with transamounts
                    transToAttach.TransNetAmount = sTransactorTransaction.TransNetAmount;
                    transToAttach.TransFpaAmount = sTransactorTransaction.TransFpaAmount;
                    transToAttach.TransDiscountAmount = sTransactorTransaction.TransDiscountAmount;
                    transToAttach.TransExpensesAmount = 0;
                    _context.Entry(transToAttach).State = EntityState.Modified;
                    //----------------------------------------------
                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                }

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods
                        .FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    return new NotFoundObjectResult(new
                    {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
                {
                    var autoPaySeriesId = transToAttach.SellDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0)
                    {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs
                                .FirstOrDefaultAsync(p =>
                                    p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            return new NotFoundObjectResult(new
                            {
                                error = "AutoPayOff series not found"
                            });
                        }

                        var transactor = await _context.Transactors
                            .Where(p => p.Id == docTrans.TransactorId)
                            .SingleOrDefaultAsync();
                        var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(docTrans);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";

                        //Ετσι δεν μεταφέρει το Id απο το docTrans
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);
                        sTransactorTransaction.TransactorId = docTrans.TransactorId;

                        sTransactorTransaction.TransTransactorDocTypeId =
                            transTransactorPayOffSeries.TransTransactorDocTypeDefId;
                        sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                        sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                        sTransactorTransaction.Etiology = transTransactorEtiology;
                        sTransactorTransaction.CreatorId = docId;
                        sTransactorTransaction.CreatorSectionId = sectionId;
                        await _context.Entry(transTransactorPayOffSeries)
                            .Reference(t => t.TransTransactorDocTypeDef)
                            .LoadAsync();
                        var transTransactorDocTypeDef = transTransactorPayOffSeries.TransTransactorDocTypeDef;

                        #region Section Management

                        if (transTransactorDocTypeDef.SectionId == 0)
                        {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else
                        {
                            sTransactorTransaction.SectionId = transTransactorDocTypeDef.SectionId;
                        }

                        #endregion

                        await _context.Entry(transTransactorDocTypeDef)
                            .Reference(t => t.TransTransactorDef)
                            .LoadAsync();
                        var transPaymentTransactorDef = transTransactorDocTypeDef.TransTransactorDef;
                        ActionHandlers.TransactorFinAction(transPaymentTransactorDef.FinancialTransAction,
                            sTransactorTransaction);

                        await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return new BadRequestObjectResult(new
                            {
                                error = e.Message + " " + msg
                            });
                        }

                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0)
                        {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0)
                            {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null)
                                {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null)
                                    {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {docTrans.Etiology} ";


                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction
                                        {
                                            TransDate = docTrans.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = docTrans.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = docTrans.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet -
                                                     sTransactorTransaction.AmountDiscount +
                                                     sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e)
                                        {
                                             if (ownsTransaction) await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return new BadRequestObjectResult(new
                                            {
                                                error = e.Message + " " + msg
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try
                        {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new SellDocTransPaymentMapping()
                            {
                                SellDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                             sTransactorTransaction.AmountDiscount
                            };
                            await _context.SellDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e)
                        {
                             if (ownsTransaction) await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return new BadRequestObjectResult(new
                            {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }

                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0)
                {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs
                            .FirstOrDefaultAsync(p =>
                                p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null)
                    {
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else
                {
                    noWarehouseTrans = true;
                }

                foreach (var docLine in docTrans.SellDocLines)
                {
                    var warehouseItemId = docLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null)
                    {
                        //Handle error
                         if (ownsTransaction) await transaction.RollbackAsync();
                        return new NotFoundObjectResult(new
                        {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    var transUnitId = docLine.TransactionUnitId;
                    var transUnitFactor = docLine.TransactionUnitFactor;
                    decimal transPrice = docLine.TransUnitPrice;
                    double transUnits = docLine.TransactionQuantity;
                    decimal unitPrice = docLine.Price;
                    decimal units = (decimal)docLine.Q1;
                    decimal fpaRate = (decimal)docLine.FpaRate;
                    decimal discountRate = (decimal)docLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    var lineAmounts = new DocLineFinancialActionAmounts
                    {
                        AmountNet = lineNetAmount,
                        AmountFpa = lineFpaAmount,
                        AmountDiscount = lineDiscountAmount,
                        AmountExpenses = 0
                    };
                    ActionHandlers.DocLineFinAction(transTransactorDef.FinancialTransAction, lineAmounts);
                    var sellDocLine = new SellDocLine
                    {
                        UnitPrice = unitPrice,
                        AmountFpa = lineFpaAmount,
                        AmountNet = lineNetAmount,
                        AmountDiscount = lineDiscountAmount,
                        DiscountRate = discountRate,
                        FpaRate = fpaRate,
                        WarehouseItemId = docLine.WarehouseItemId,
                        Quontity1 = docLine.Q1,
                        Quontity2 = docLine.Q2,
                        PrimaryUnitId = docLine.MainUnitId,
                        SecondaryUnitId = docLine.SecUnitId,
                        Factor = docLine.Factor,
                        SellDocumentId = docId,
                        Etiology = transToAttach.Etiology,
                        TransactionUnitId = transUnitId,
                        TransactionQuantity = transUnits,
                        TransUnitPrice = transPrice,
                        TransactionUnitFactor = transUnitFactor,
                        TransNetAmount = lineAmounts.TransNetAmount,
                        TransFpaAmount = lineAmounts.TransFpaAmount,
                        TransDiscountAmount = lineAmounts.TransDiscountAmount,
                        TransExpensesAmount = lineAmounts.TransExpensesAmount
                    };
                    //_context.Entry(transToAttach).Entity
                    transToAttach.SellDocLines.Add(sellDocLine);

                    #endregion

                    if (!noWarehouseTrans)
                    {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction
                        {
                            FpaRate = fpaRate,
                            DiscountRate = discountRate,
                            UnitPrice = unitPrice,
                            AmountDiscount = lineDiscountAmount,
                            AmountNet = lineNetAmount,
                            AmountFpa = lineFpaAmount,
                            CompanyId = transToAttach.CompanyId,
                            Etiology = transToAttach.Etiology,
                            FiscalPeriodId = transToAttach.FiscalPeriodId,
                            WarehouseItemId = warehouseItemId,
                            PrimaryUnitId = docLine.MainUnitId,
                            SecondaryUnitId = docLine.SecUnitId,
                            SectionId = sectionId,
                            CreatorId = transToAttach.Id,
                            TransDate = transToAttach.TransDate,
                            TransRefCode = transToAttach.TransRefCode,
                            UnitFactor = (decimal)docLine.Factor,
                            TransWarehouseDocSeriesId = warehouseSeriesId,
                            TransWarehouseDocTypeId = warehouseTypeId
                        };

                        ActionHandlers.ItemNatureHandler(material.WarehouseItemNature, warehouseTrans,
                            transWarehouseDef);
                        ActionHandlers.ItemInventoryActionHandler(warehouseTrans.InventoryAction, docLine.Q1,
                            docLine.Q2,
                            warehouseTrans);
                        ActionHandlers.ItemInventoryValueActionHandler(warehouseTrans.InventoryValueAction,
                            warehouseTrans);

                        await _context.WarehouseTransactions.AddAsync(warehouseTrans);

                        #endregion
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    if (ownsTransaction) await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                     if (ownsTransaction) await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return new BadRequestObjectResult(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }
            
            finally
            {
                // Dispose the transaction only if we created it
                if (ownsTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
            return new OkObjectResult(new {});
    }

    public async Task<ServiceResult> DeleteSaleDocument(int docId)
    {
        if (docId < 0)
        {
            return ServiceResult.Error("No Document Id", "ARGUMENT_ERROR");
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        var saleDocument = await _context.SellDocuments.FindAsync(docId);
        if (saleDocument == null)
        {
            return ServiceResult.Error("No Document Found", "NOT_FOUND");
        }
        try
        {
            _context.SellDocLines.RemoveRange(_context.SellDocLines.Where(p => p.SellDocumentId == docId));
            _context.TransactorTransactions.RemoveRange(_context.TransactorTransactions.Where(p => p.CreatorSectionId == saleDocument.SectionId && p.CreatorId == docId));
            _context.CashFlowAccountTransactions.RemoveRange(_context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == saleDocument.SectionId && p.CreatorId == docId));
            _context.WarehouseTransactions.RemoveRange(_context.WarehouseTransactions.Where(p => p.SectionId == saleDocument.SectionId && p.CreatorId == docId));
            _context.SellDocTransPaymentMappings.RemoveRange(_context.SellDocTransPaymentMappings.Where(p=>p.SellDocumentId==docId));
            _context.SellDocuments.Remove(saleDocument);

            await _context.SaveChangesAsync();
            // var syncDoc = await _context.SyncSaleDocuments.SingleOrDefaultAsync(p => p.ErpId == saleDocument.Id);
            // if (syncDoc is not null)
            // {
            //     var syncLog = await _context.SynchronizationLogs.SingleOrDefaultAsync(p => p.EntityId == syncDoc.Id);
            //     if (syncLog is not null)
            //     {
            //         _context.SynchronizationLogs.Remove(syncLog);
            //     }
            //
            //     _context.SyncSaleDocuments.Remove(syncDoc);
            // }

            //_context.SellDocuments.Remove(saleDocument);

           // await _context.SaveChangesAsync();
            // if (generateTestError)
            // {
            //     throw new Exception("Test Error");
            // }
            if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
            return ServiceResult.Error(msg, "ERROR");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        return ServiceResult.Ok();
    }
}