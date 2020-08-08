using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify.Helpers;

namespace GrKouk.Web.ERP.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class RecurringTransactionsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public RecurringTransactionsController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("ApplyRecTransIdList")]
        public async Task<IList<GroupActionResult>> ApplyRecTransIdList([FromBody] IdList docIds)
        {
            IList<GroupActionResult> retValList = new List<GroupActionResult>();
            foreach (var itemId in docIds.Ids)
            {
                var actionRetVal = await CreateDocFromRecTrans(itemId);
                //var statusCodeResult = actionRetVal as StatusCodeResult;

                if (actionRetVal is OkObjectResult okObjectResult)
                {
                    int? r = okObjectResult.StatusCode;

                    int statusCode = r ?? default;
                    var retVal = new GroupActionResult
                    {
                        StatusCode = statusCode
                        //StatusText = (actionRetVal as StatusCodeResult).ToString()
                    };
                    retValList.Add(retVal);
                }
                if (actionRetVal is NotFoundObjectResult notFoundObjectResult)
                {
                    int? r = notFoundObjectResult.StatusCode;

                    int statusCode = r ?? default;
                    var retVal = new GroupActionResult
                    {
                        StatusCode = statusCode
                        //StatusText = (actionRetVal as StatusCodeResult).ToString()
                    };
                    retValList.Add(retVal);
                }
                if (actionRetVal is BadRequestObjectResult badRequestObjectResult)
                {
                    int? r = badRequestObjectResult.StatusCode;

                    int statusCode = r ?? default;
                    var retVal = new GroupActionResult
                    {
                        StatusCode = statusCode
                        //StatusText = (actionRetVal as StatusCodeResult).ToString()
                    };
                    retValList.Add(retVal);
                    Debug.Print("Bad request result");
                }
            }

            return retValList;
        }

        [HttpGet(template: "CreateDocFromRecTrans")]
        public async Task<IActionResult> CreateDocFromRecTrans(int id)
        {
            if (id <= 0)
            {
                return BadRequest(error: new { Message = "Id must be grater than 0" });
            }

            var recDef = await _context.RecurringTransDocs
                .Include(p => p.DocLines)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (recDef == null)
            {
                return NotFound(
                    new
                    {
                        Message = "Recurring transaction definition not found"
                    });
            }

            IActionResult actionResult;
            switch (recDef.RecurringDocType)
            {
                case RecurringDocTypeEnum.BuyType:
                    var buyDocDto = _mapper.Map<BuyDocCreateAjaxDto>(recDef);
                    var buyUpdController = new MaterialsController(_context, _mapper);
                    actionResult = await CreateBuyDocFromRecTrans(buyDocDto, recDef);
                    break;
                case RecurringDocTypeEnum.SellType:
                    var sellDocDto = _mapper.Map<SellDocCreateAjaxDto>(recDef);
                    var sellUpdController = new MaterialsController(_context, _mapper);
                    actionResult = await CreateSellDocFromRecTrans(sellDocDto, recDef);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var rsJson = actionResult.ToJson();

            return Ok(actionResult);
        }

        private async Task<IActionResult> CreateBuyDocFromRecTrans(BuyDocCreateAjaxDto data,
            RecurringTransDoc recurringTransDoc)
        {
            const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            // bool noSupplierTrans = false;
            bool noWarehouseTrans = false;

            #region BuyDocument

            BuyDocCreateAjaxNoLinesDto transToAttachNoLines;
            BuyDocument transToAttach;
            DateTime dateOfTrans;

            if (data == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            try
            {
                transToAttachNoLines = _mapper.Map<BuyDocCreateAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<BuyDocument>(transToAttachNoLines);
                dateOfTrans = data.TransDate;
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    error = e.Message
                });
            }

            var tr = _context.Database.CurrentTransaction;
            using (var transaction = _context.Database.BeginTransaction())
            {
                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null)
                {
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new
                    {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.BuyDocSeriesId);

                if (docSeries is null)
                {
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new
                    {
                        error = "Buy Doc Series not found"
                    });
                }

                await _context.Entry(docSeries).Reference(t => t.BuyDocTypeDef).LoadAsync();
                var docTypeDef = docSeries.BuyDocTypeDef;
                //await _context.Entry(docTypeDef)
                //      .Reference(t => t.TransSupplierDef)
                //      .LoadAsync();
                await _context.Entry(docTypeDef)
                    .Reference(t => t.TransTransactorDef)
                    .LoadAsync();

                await _context.Entry(docTypeDef).Reference(t => t.TransWarehouseDef)
                    .LoadAsync();

                #region Section Management

                int sectionId = 0;
                if (docTypeDef.SectionId == 0)
                {
                    var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new
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

                //var transSupplierDef = docTypeDef.TransSupplierDef;
                var transTransactorDef = docTypeDef.TransTransactorDef;
                var transWarehouseDef = docTypeDef.TransWarehouseDef;

                transToAttach.SectionId = sectionId;
                transToAttach.FiscalPeriodId = fiscalPeriod.Id;
                transToAttach.BuyDocTypeId = docSeries.BuyDocTypeDefId;
                //transToAttach.Id = 0;
                _context.BuyDocuments.Add(transToAttach);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                var docId = _context.Entry(transToAttach).Entity.Id;


                if (transTransactorDef.DefaultDocSeriesId > 0)
                {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new
                        {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                    sTransactorTransaction.TransactorId = data.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, sTransactorTransaction);
                    sTransactorTransaction.Id = 0;
                    _context.TransactorTransactions.Add(sTransactorTransaction);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Rollback();
                        string msg = e.InnerException.Message;
                        return BadRequest(new
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
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new
                    {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
                {
                    var autoPaySeriesId = transToAttach.BuyDocSeries.PayoffSeriesId;
                    if (autoPaySeriesId > 0)
                    {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new
                            {
                                error = "AutoPayOff series not found"
                            });
                        }

                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                        sTransactorTransaction.TransactorId = data.TransactorId;
                        
                        sTransactorTransaction.TransTransactorDocTypeId =
                            transTransactorPayOffSeries.TransTransactorDocTypeDefId;
                        sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                        sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                        sTransactorTransaction.Etiology = "AutoPayOff";
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
                        _context.TransactorTransactions.Add(sTransactorTransaction);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new
                            {
                                error = e.Message + " " + msg
                            });
                        }
                        try
                        {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new BuyDocTransPaymentMapping()
                            {
                                BuyDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet+sTransactorTransaction.AmountFpa-sTransactorTransaction.AmountDiscount
                            };
                            _context.BuyDocTransPaymentMappings.Add(payOffMapping);
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new
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
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new
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

                foreach (var dataBuyDocLine in data.BuyDocLines)
                {
                    var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null)
                    {
                        //Handle error
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new
                        {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    var buyMaterialLine = new BuyDocLine();
                    decimal unitPrice = dataBuyDocLine.Price;
                    decimal units = (decimal)dataBuyDocLine.Q1;
                    decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                    decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    buyMaterialLine.UnitPrice = unitPrice;
                    buyMaterialLine.AmountFpa = lineFpaAmount;
                    buyMaterialLine.AmountNet = lineNetAmount;
                    buyMaterialLine.AmountDiscount = lineDiscountAmount;
                    buyMaterialLine.DiscountRate = discountRate;
                    buyMaterialLine.FpaRate = fpaRate;
                    buyMaterialLine.WarehouseItemId = dataBuyDocLine.WarehouseItemId;
                    buyMaterialLine.Quontity1 = dataBuyDocLine.Q1;
                    buyMaterialLine.Quontity2 = dataBuyDocLine.Q2;
                    buyMaterialLine.PrimaryUnitId = dataBuyDocLine.MainUnitId;
                    buyMaterialLine.SecondaryUnitId = dataBuyDocLine.SecUnitId;
                    buyMaterialLine.Factor = dataBuyDocLine.Factor;
                    buyMaterialLine.BuyDocumentId = docId;
                    buyMaterialLine.Etiology = transToAttach.Etiology;
                    //_context.Entry(transToAttach).Entity
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
                        _context.WarehouseTransactions.Add(warehouseTrans);

                        #endregion
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                #endregion

                var curDate = recurringTransDoc.NextTransDate;
                var nextDate = GetNextTransactionDate(curDate, recurringTransDoc.RecurringFrequency);
                recurringTransDoc.NextTransDate = nextDate;
                _context.Entry(recurringTransDoc).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok();
        }


        private async Task<IActionResult> CreateSellDocFromRecTrans(SellDocCreateAjaxDto data, RecurringTransDoc recurringTransDoc)
        {
            const string sectionCode = "SYS-SELL-COMBINED-SCN";

            bool noWarehouseTrans = false;

            SellDocCreateAjaxNoLinesDto transToAttachNoLines;
            SellDocument transToAttach;
            DateTime dateOfTrans;

            if (data == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            try
            {
                transToAttachNoLines = _mapper.Map<SellDocCreateAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<SellDocument>(transToAttachNoLines);
                transToAttach.SalesChannelId = 1;
                dateOfTrans = data.TransDate;
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    error = e.Message
                });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null)
                {
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new
                    {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.SellDocSeriesId);

                if (docSeries is null)
                {
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new
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
                    var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new
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
                _context.SellDocuments.Add(transToAttach);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                var docId = _context.Entry(transToAttach).Entity.Id;


                if (transTransactorDef.DefaultDocSeriesId > 0)
                {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new
                        {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                    sTransactorTransaction.TransactorId = data.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction,
                        sTransactorTransaction);

                    _context.TransactorTransactions.Add(sTransactorTransaction);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        string msg = e.InnerException.Message;
                        return BadRequest(new
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
                    transaction.Rollback();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new
                    {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto)
                {
                    var autoPaySeriesId = transToAttach.SellDocSeries.PayoffSeriesId;
                    if (autoPaySeriesId > 0)
                    {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new
                            {
                                error = "AutoPayOff series not found"
                            });
                        }

                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                        sTransactorTransaction.TransactorId = data.TransactorId;
                        
                        sTransactorTransaction.TransTransactorDocTypeId =
                            transTransactorPayOffSeries.TransTransactorDocTypeDefId;
                        sTransactorTransaction.TransTransactorDocSeriesId = transTransactorPayOffSeries.Id;
                        sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                        sTransactorTransaction.Etiology = "AutoPayOff";
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

                        _context.TransactorTransactions.Add(sTransactorTransaction);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new
                            {
                                error = e.Message + " " + msg
                            });
                        }
                        try
                        {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new SellDocTransPaymentMapping()
                            {
                                SellDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet+sTransactorTransaction.AmountFpa-sTransactorTransaction.AmountDiscount
                            };
                            _context.SellDocTransPaymentMappings.Add(payOffMapping);
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new
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
                        await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new
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

                foreach (var docLine in data.SellDocLines)
                {
                    var warehouseItemId = docLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null)
                    {
                        //Handle error
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new
                        {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    var sellDocLine = new SellDocLine();
                    decimal unitPrice = docLine.Price;
                    decimal units = (decimal)docLine.Q1;
                    decimal fpaRate = (decimal)docLine.FpaRate;
                    decimal discountRate = (decimal)docLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    sellDocLine.UnitPrice = unitPrice;
                    sellDocLine.AmountFpa = lineFpaAmount;
                    sellDocLine.AmountNet = lineNetAmount;
                    sellDocLine.AmountDiscount = lineDiscountAmount;
                    sellDocLine.DiscountRate = discountRate;
                    sellDocLine.FpaRate = fpaRate;
                    sellDocLine.WarehouseItemId = docLine.WarehouseItemId;
                    sellDocLine.Quontity1 = docLine.Q1;
                    sellDocLine.Quontity2 = docLine.Q2;
                    sellDocLine.PrimaryUnitId = docLine.MainUnitId;
                    sellDocLine.SecondaryUnitId = docLine.SecUnitId;
                    sellDocLine.Factor = docLine.Factor;
                    sellDocLine.SellDocumentId = docId;
                    sellDocLine.Etiology = transToAttach.Etiology;
                    //_context.Entry(transToAttach).Entity
                    transToAttach.SellDocLines.Add(sellDocLine);

                    #endregion

                    if (!noWarehouseTrans)
                    {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction();
                        warehouseTrans.FpaRate = fpaRate;
                        warehouseTrans.DiscountRate = discountRate;
                        warehouseTrans.UnitPrice = unitPrice;
                        warehouseTrans.AmountDiscount = lineDiscountAmount;
                        warehouseTrans.AmountNet = lineNetAmount;
                        warehouseTrans.AmountFpa = lineFpaAmount;
                        warehouseTrans.CompanyId = transToAttach.CompanyId;
                        warehouseTrans.Etiology = transToAttach.Etiology;
                        warehouseTrans.FiscalPeriodId = transToAttach.FiscalPeriodId;

                        warehouseTrans.WarehouseItemId = warehouseItemId;
                        warehouseTrans.PrimaryUnitId = docLine.MainUnitId;
                        warehouseTrans.SecondaryUnitId = docLine.SecUnitId;
                        warehouseTrans.SectionId = sectionId;
                        warehouseTrans.CreatorId = transToAttach.Id;
                        warehouseTrans.TransDate = transToAttach.TransDate;
                        warehouseTrans.TransRefCode = transToAttach.TransRefCode;
                        warehouseTrans.UnitFactor = (decimal)docLine.Factor;

                        warehouseTrans.TransWarehouseDocSeriesId = warehouseSeriesId;
                        warehouseTrans.TransWarehouseDocTypeId = warehouseTypeId;
                        ActionHandlers.ItemNatureHandler(material.WarehouseItemNature, warehouseTrans,
                            transWarehouseDef);
                        ActionHandlers.ItemInventoryActionHandler(warehouseTrans.InventoryAction, docLine.Q1,
                            docLine.Q2,
                            warehouseTrans);
                        ActionHandlers.ItemInventoryValueActionHandler(warehouseTrans.InventoryValueAction,
                            warehouseTrans);

                        _context.WarehouseTransactions.Add(warehouseTrans);

                        #endregion
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }

                var curDate = recurringTransDoc.NextTransDate;
                var nextDate = GetNextTransactionDate(curDate, recurringTransDoc.RecurringFrequency);
                recurringTransDoc.NextTransDate = nextDate;
                _context.Entry(recurringTransDoc).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new
                    {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok();
        }

        private DateTime GetNextTransactionDate(DateTime currentDate, string frequency)
        {
            DateTime nextDate = currentDate;
            switch (frequency)
            {
                case "1D":
                    nextDate = currentDate.AddDays(1);
                    break;
                case "7D":
                    nextDate = currentDate.AddDays(7);
                    break;
                case "1M":
                    nextDate = currentDate.AddMonths(1);
                    break;
                case "2M":
                    nextDate = currentDate.AddMonths(2);
                    break;
                case "3M":
                    nextDate = currentDate.AddMonths(3);
                    break;
                case "6M":
                    nextDate = currentDate.AddMonths(6);
                    break;
                case "1Y":
                    nextDate = currentDate.AddYears(1);
                    break;
            }

            return nextDate;
        }
    }
}