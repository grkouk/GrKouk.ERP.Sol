using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Controllers {
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public MaterialsController(ApiDbContext context, IMapper mapper) {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/WarehouseItems
        [HttpGet]
        public IEnumerable<WarehouseItem> GetMaterials() {
            return _context.WarehouseItems;
        }

        [HttpGet("SetCompanyInSession")]
        public IActionResult CompanyInSession(string companyId) {
            HttpContext.Session.SetString("CompanyId", companyId);
            return Ok(new { });
        }
        [HttpGet("SetTransactorInSession")]
        public IActionResult TransactorInSession(string transactorId) {
            HttpContext.Session.SetString("TransactorId", transactorId);
            return Ok(new { });
        }
        [HttpGet("SetBuySeriesInSession")]
        public IActionResult BuySeriesInSession(string seriesId) {
            HttpContext.Session.SetString("BuySeriesId", seriesId);
            return Ok(new { });
        }

        [HttpGet("SetDocSeriesInSession")]
        public IActionResult DocSeriesInSession(string seriesId) {
            HttpContext.Session.SetString("SeriesId", seriesId);
            return Ok(new { });
        }

        [HttpGet("SetSaleSeriesInSession")]
        public IActionResult SaleSeriesInSession(string seriesId) {
            HttpContext.Session.SetString("SalesSeriesId", seriesId);
            return Ok(new { });
        }

        [HttpGet("SeekCompanyBarcode")]
        public async Task<IActionResult> GetCompanyMaterialFromBarcode(string barcode, int companyId) {
            var materials = await _context.WrItemCodes
                .Include(p => p.WarehouseItem).ThenInclude(p => p.FpaDef)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.MainMeasureUnit)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.SecondaryMeasureUnit)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.BuyMeasureUnit)
                .Where(p => p.Code == barcode && p.CodeType == WarehouseItemCodeTypeEnum.CodeTypeEnumBarcode &&
                            p.CompanyId == companyId)
                .ToListAsync();

            if (materials == null) {
                return NotFound(new {
                    Error = "WarehouseItem Not Found"
                });
            }

            if (materials.Count > 1) {
                return NotFound(new {
                    Error = "More than one material found"
                });
            }

            var material = materials[0].WarehouseItem;

            var usedUnit = materials[0].CodeUsedUnit;
            double unitFactor = materials[0].RateToMainUnit;
            string unitToUse;
            switch (usedUnit) {
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumMain:
                    unitFactor = 1;
                    unitToUse = "MAIN";
                    break;
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumSecondary:
                    unitToUse = "SEC";
                    break;
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumBuy:
                    unitToUse = "BUY";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var lastPr = await _context.WarehouseTransactions.Where(m => m.Id == material.Id)
                .Select(k => new {
                    LastPrice = k.UnitPrice
                }).FirstOrDefaultAsync();

            var lastPrice = lastPr?.LastPrice ?? 0;

            return Ok(new {
                material.Id,
                material.Name,
                fpaId = material.FpaDefId,
                lastPrice,
                fpaRate = material.FpaDef.Rate,
                unitToUse,
                Factor = unitFactor,
                mainUnitId = material.MainMeasureUnitId,
                secUnitId = material.SecondaryMeasureUnitId,
                buyUnitId = material.BuyMeasureUnitId,
                mainUnitCode = material.MainMeasureUnit.Code,
                secUnitCode = material.SecondaryMeasureUnit.Code,
                buyUnitCode = material.BuyMeasureUnit.Code,
                factorSeq = material.SecondaryUnitToMainRate,
                factorBuy = material.BuyUnitToMainRate,
            });
        }

        [HttpGet("SeekBarcode")]
        public async Task<IActionResult> GetMaterialFromBarcode(string barcode, string companyId, string transactorId) {
            var materials = _context.WrItemCodes
                .Include(p => p.WarehouseItem).ThenInclude(p => p.FpaDef)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.MainMeasureUnit)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.SecondaryMeasureUnit)
                .Include(p => p.WarehouseItem).ThenInclude(p => p.BuyMeasureUnit)
                .Where(p => p.Code == barcode && p.CodeType == WarehouseItemCodeTypeEnum.CodeTypeEnumBarcode);
            int transId = 0;
            if (!string.IsNullOrEmpty(transactorId)) {
                if (int.TryParse(transactorId, out transId)) {
                    if (transId > 0) {
                        materials = materials.Where(p => p.TransactorId == transId || p.TransactorId == 0);
                    }
                }
            }

            int compId = 0;
            if (!string.IsNullOrEmpty(companyId)) {
                if (int.TryParse(companyId, out compId)) {
                    if (compId > 0) {
                        materials = materials.Where(p => p.CompanyId == compId || p.CompanyId == 1);
                    }
                }
            }

            var materialList = await materials.ToListAsync();

            if (materialList == null) {
                return NotFound(new {
                    Error = "WarehouseItem Not Found"
                });
            }

            if (materialList.Count == 0) {
                return NotFound(new {
                    Error = "WarehouseItem Not Found"
                });
            }

            List<WrItemCode> retMaterials;
            if (materialList.Count > 1) {
                //return NotFound(new
                //{
                //    Error = "More than one material found"
                //});
                retMaterials = materialList.OrderByDescending(p => p.TransactorId).ToList();
            }
            else {
                retMaterials = materialList;
            }

            var material = retMaterials[0].WarehouseItem;

            var usedUnit = retMaterials[0].CodeUsedUnit;
            double unitFactor;
            string unitToUse;
            switch (usedUnit) {
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumMain:
                    unitFactor = 1;
                    unitToUse = "MAIN";
                    break;
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumSecondary:
                    unitFactor = retMaterials[0].RateToMainUnit;
                    unitToUse = "SEC";
                    break;
                case WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumBuy:
                    unitFactor = retMaterials[0].RateToMainUnit;
                    unitToUse = "BUY";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var lastPr = await _context.WarehouseTransactions.Where(m => m.Id == material.Id)
                .Select(k => new {
                    LastPrice = k.UnitPrice
                }).FirstOrDefaultAsync();

            var lastPrice = lastPr?.LastPrice ?? 0;

            return Ok(new {
                material.Id,
                material.Name,
                fpaId = material.FpaDefId,
                lastPrice,
                fpaRate = material.FpaDef.Rate,
                unitToUse,
                Factor = unitFactor,
                mainUnitId = material.MainMeasureUnitId,
                secUnitId = material.SecondaryMeasureUnitId,
                buyUnitId = material.BuyMeasureUnitId,
                mainUnitCode = material.MainMeasureUnit.Code,
                secUnitCode = material.SecondaryMeasureUnit.Code,
                buyUnitCode = material.BuyMeasureUnit.Code,
                factorSeq = material.SecondaryUnitToMainRate,
                factorBuy = material.BuyUnitToMainRate,
            });
        }

        [HttpGet("SearchWarehouseItemsForBuy")]
        public async Task<IActionResult> GetWarehouseItemsForBuy(string term) {
            var sessionCompanyId = HttpContext.Session.GetString("CompanyId");
            var sessionSeriesId = HttpContext.Session.GetString("BuySeriesId");
            IQueryable<WarehouseItem> fullListIq = _context.WarehouseItems;
            if (sessionCompanyId != null) {
                bool isInt = int.TryParse(sessionCompanyId, out var companyId);
                if (isInt) {
                    if (companyId > 1) {
                        //Not all companies 
                        fullListIq = fullListIq.Where(p => p.CompanyId == companyId || p.CompanyId == 1);
                    }
                }
            }

            if (sessionSeriesId != null) {
                bool isInt = int.TryParse(sessionSeriesId, out int seriesId);
                var series = await _context.BuyDocSeriesDefs
                    .Include(p => p.BuyDocTypeDef)
                    .SingleOrDefaultAsync(p => p.Id == seriesId);
                if (series != null) {
                    var docType = series.BuyDocTypeDef;
                    var itemNatures = docType.SelectedWarehouseItemNatures;
                    if (!string.IsNullOrEmpty(itemNatures)) {
                        var natures = Array.ConvertAll(docType.SelectedWarehouseItemNatures.Split(","), int.Parse);
                        //var natures = docType.SelectedWarehouseItemNatures;
                        fullListIq = fullListIq.Where(p => natures.Contains((int)p.WarehouseItemNature));
                    }
                }
            }

            fullListIq = fullListIq.Where(p => p.Active);
            fullListIq = fullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            var m = await fullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new { label = p.Label, value = p.Id })

                .ToListAsync();

            var materials = m.OrderBy(p => p.label);


            return Ok(materials);
        }
        [HttpGet("AutoCompleteProductsBySupplierCode")]
        public async Task<IActionResult> GetAutoCompleteProductsBySupplierCode(string term) {
            var sessionCompanyId = HttpContext.Session.GetString("CompanyId");
            var sessionSeriesId = HttpContext.Session.GetString("BuySeriesId");
            var sessionTransactorId = HttpContext.Session.GetString("TransactorId");
            IQueryable<WarehouseItem> fullListIq = _context.WarehouseItems.Include(x => x.WarehouseItemCodes);
            int companyId = 0;
            if (sessionCompanyId != null) {
                int.TryParse(sessionCompanyId, out companyId);
            }


            if (sessionTransactorId != null) {
                int.TryParse(sessionTransactorId, out int transactorId);

                if (transactorId > 1) {
                    //var transactorsList = Array.ConvertAll(transactorId.ToString().Split(","), int.Parse);
                    if (companyId > 0) {
                        fullListIq = fullListIq
                            .Where(m => m.WarehouseItemCodes
                                .Any(x => x.TransactorId == transactorId && x.CompanyId == companyId && x.Code.Contains(term)));

                    }
                    else {
                        fullListIq = fullListIq
                            .Where(m => m.WarehouseItemCodes
                                .Any(x => x.TransactorId == transactorId && x.Code.Contains(term)));

                    }

                }

            }
            fullListIq = fullListIq.Where(p => p.Active);
            //fullListIq = fullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));

            var materials = await fullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new { label = p.Label, value = p.Id })
                .ToListAsync();

            if (materials == null) {
                return NotFound();
            }

            return Ok(materials);
        }
        [HttpGet("SearchWarehouseItemsForSale")]
        public async Task<IActionResult> GetWarehouseItemsForSale(string term) {
            var sessionCompanyId = HttpContext.Session.GetString("CompanyId");
            var sessionSeriesId = HttpContext.Session.GetString("BuySeriesId");
            IQueryable<WarehouseItem> fullListIq = _context.WarehouseItems;
            if (sessionCompanyId != null) {
                bool isInt = int.TryParse(sessionCompanyId, out var companyId);
                if (isInt) {
                    if (companyId > 1) {
                        //Not all companies 
                        fullListIq = fullListIq.Where(p => p.CompanyId == companyId || p.CompanyId == 1);
                    }
                }
            }

            if (sessionSeriesId != null) {
                bool isInt = int.TryParse(sessionSeriesId, out int seriesId);
                var series = await _context.SellDocSeriesDefs
                    .Include(p => p.SellDocTypeDef)
                    .SingleOrDefaultAsync(p => p.Id == seriesId);
                if (series != null) {
                    var docType = series.SellDocTypeDef;
                    var itemNatures = docType.SelectedWarehouseItemNatures;
                    if (!string.IsNullOrEmpty(itemNatures)) {
                        var natures = Array.ConvertAll(docType.SelectedWarehouseItemNatures.Split(","), int.Parse);
                        //var natures = docType.SelectedWarehouseItemNatures;
                        fullListIq = fullListIq.Where(p => natures.Contains((int)p.WarehouseItemNature));
                    }
                }
            }

            fullListIq = fullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            //var materials = await _context.WarehouseItems.Where(p => p.Name.Contains(term) )
            //    .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
            //    .Select(p => new { label = p.Label, value = p.Id }).ToListAsync();
            var materials = await fullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new { label = p.Label, value = p.Id }).ToListAsync();
            if (materials == null) {
                return NotFound();
            }

            return Ok(materials);
        }

        [HttpGet("SearchForServices")]
        public async Task<IActionResult> GetServices(string term) {
            var materials = await _context.WarehouseItems.Where(p =>
                    p.Name.Contains(term) &&
                    p.WarehouseItemNature == WarehouseItemNatureEnum.WarehouseItemNatureService)
                .Select(p => new { label = p.Name, value = p.Id }).ToListAsync();

            if (materials == null) {
                return NotFound();
            }

            return Ok(materials);
        }

        [HttpGet("SearchForExpenses")]
        public async Task<IActionResult> GetExpenses(string term) {
            var materials = await _context.WarehouseItems.Where(p =>
                    p.Name.Contains(term) &&
                    p.WarehouseItemNature == WarehouseItemNatureEnum.WarehouseItemNatureExpense)
                .Select(p => new { label = p.Name, value = p.Id }).ToListAsync();

            if (materials == null) {
                return NotFound();
            }

            return Ok(materials);
        }
        [HttpGet("companyBaseCurrencyInfo")]
        public async Task<IActionResult> CompanyBaseCurrencyInfoAsync(int companyId) {
            if (companyId == 0) {
                return BadRequest(new {
                    error = "No company Id provided "
                });
            }
            var theCompany = await _context.Companies
                .Include(p => p.Currency)
                .Where(p => p.Id == companyId)
                .SingleOrDefaultAsync();

            if (theCompany == null) {
                return NotFound(new {
                    error = "Company not found "
                });
            }

            var response = new {
                CurrencyCode = theCompany.Currency.Code,
                CurrencyLocale = theCompany.Currency.DisplayLocale


            };
            return Ok(response);
        }
        [HttpGet("CashFlowAccountsForCompany")]
        public async Task<IActionResult> CashFlowAccountsForCompanyAsync(int companyId) {
            //Thread.Sleep(10000);
            if (companyId == 0) {
                return BadRequest(new {
                    error = "No company Id provided"
                });
            }
            var theCompany = await _context.Companies
                .Include(p => p.Currency)
                .Where(p => p.Id == companyId)
                .SingleOrDefaultAsync();

            if (theCompany == null) {
                return NotFound(new {
                    error = "Company not found "
                });
            }

            var accountsForCompany = await _context.CashFlowAccountCompanyMappings
                .Include(p => p.CashFlowAccount)
                .Where(p => p.CompanyId == companyId)
                .Select(x => new {
                    value = x.CashFlowAccountId,
                    text = x.CashFlowAccount.Name
                }
                ).ToListAsync();
            var response = new {
                CurrencyCode = theCompany.Currency.Code,
                CurrencyLocale = theCompany.Currency.DisplayLocale,
                Accounts = accountsForCompany

            };
            return Ok(response);
        }
        [HttpGet("productdata")]
        public async Task<IActionResult> GetProductDataAsync(int warehouseItemId, int transactorId, int companyId) {
            if (warehouseItemId == 0) {
                return BadRequest(new {
                    error = "No warehouse item Id provided "
                });
            }
            //Get last price for product 
            IQueryable<WarehouseTransaction> lastPriceQr = _context.WarehouseTransactions;
            lastPriceQr = lastPriceQr.Where(m => m.WarehouseItemId == warehouseItemId);
            if (companyId > 0) {
                lastPriceQr = lastPriceQr.Where(m => m.CompanyId == companyId);
            }

            var lastPr = await lastPriceQr
                .OrderByDescending(p => p.TransDate)
                .Select(k => new {
                    LastPrice = k.UnitPrice
                })
                .FirstOrDefaultAsync();

            var lastPrice = lastPr?.LastPrice ?? 0;
            var materialData = await _context.WarehouseItems
                .Include(p => p.BuyMeasureUnit)
                .Include(p => p.MainMeasureUnit)
                .Include(p => p.SecondaryMeasureUnit)
                .Include(p => p.FpaDef)
                .Include(p => p.WarehouseItemCodes)
                .Where(p => p.Id == warehouseItemId && p.Active)
                .FirstOrDefaultAsync();


            if (materialData == null) {
                return NotFound(new {
                    error = "WarehouseItem not found "
                });
            }
            var unitList = new List<ProductUnit>
            {
                new ProductUnit()
                {
                    UnitId = materialData.MainMeasureUnitId,
                    UnitCode = materialData.MainMeasureUnit.Code,
                    UnitName = materialData.MainMeasureUnit.Name,
                    UnitType = UnitTypeEnum.BaseUnitType,
                    IsDefault = false,
                    UnitFactor = 1
                },
                new ProductUnit()
                {
                    UnitId = materialData.SecondaryMeasureUnitId,
                    UnitCode = materialData.SecondaryMeasureUnit.Code,
                    UnitName = materialData.SecondaryMeasureUnit.Name,
                    UnitType = UnitTypeEnum.SecondaryUnitType,
                    IsDefault = false,
                    UnitFactor = materialData.SecondaryUnitToMainRate
                },
                new ProductUnit()
                {
                    UnitId = materialData.BuyMeasureUnitId,
                    UnitCode = materialData.BuyMeasureUnit.Code,
                    UnitName = materialData.BuyMeasureUnit.Name,
                    UnitType = UnitTypeEnum.BuyUnitType,
                    IsDefault = true,
                    UnitFactor = materialData.BuyUnitToMainRate
                }
            };
            //get special codes
            //foreach (var spCode in materialData.WarehouseItemCodes)
            //{
            //    unitList.Add(new ProductUnit()
            //    {
            //        UnitId = spCode.Id,
            //        UnitCode = spCode.BuyMeasureUnit.Code,
            //        UnitName = spCode.BuyMeasureUnit.Name,
            //        UnitType = UnitTypeEnum.BuyUnitType,
            //        UnitFactor = spCode.BuyUnitToMainRate
            //    });
            //}
            var response = new ProductInfoResponse() {
                WarehouseItemName = materialData.Name,
                FpaId = materialData.FpaDefId,
                FpaRate = materialData.FpaDef.Rate,
                LastPrice = lastPrice,
                PriceNetto = materialData.PriceNetto,
                PriceBrutto = materialData.PriceBrutto,
                MainUnitId = materialData.MainMeasureUnitId,
                MainUnitCode = materialData.MainMeasureUnit.Code,
                SecondaryUnitId = materialData.SecondaryMeasureUnitId,
                SecondaryUnitCode = materialData.SecondaryMeasureUnit.Code,
                SecondaryFactor = materialData.SecondaryUnitToMainRate,
                ProductUnits = unitList
            };
            return Ok(response);
        }
        [HttpGet("materialdata")]
        public async Task<IActionResult> GetMaterialData(int warehouseItemId) {
            //TODO: Να βρίσκει τιμές μόνο για κινήσεις αγοράς ισως LasrPriceImport LastPriceExport???
            var lastPr = await _context.WarehouseTransactions.OrderByDescending(p => p.TransDate)
                .Where(m => m.WarehouseItemId == warehouseItemId)
                .Select(k => new {
                    LastPrice = k.UnitPrice
                }).FirstOrDefaultAsync();

            var lastPrice = lastPr?.LastPrice ?? 0;

            var materialData = await _context.WarehouseItems
                .Where(p => p.Id == warehouseItemId && p.Active)
                .Select(p => new {
                    mainUnitId = p.MainMeasureUnitId,
                    secUnitId = p.SecondaryMeasureUnitId,
                    buyUnitId = p.BuyMeasureUnitId,
                    mainUnitCode = p.MainMeasureUnit.Code,
                    secUnitCode = p.SecondaryMeasureUnit.Code,
                    buyUnitCode = p.BuyMeasureUnit.Code,
                    factorSeq = p.SecondaryUnitToMainRate,
                    factorBuy = p.BuyUnitToMainRate,
                    fpaId = p.FpaDefId,
                    lastPrice,
                    priceNetto = p.PriceNetto,
                    priceBrutto = p.PriceBrutto,
                    warehouseItemName = p.Name,
                    fpaRate = p.FpaDef.Rate
                }).FirstOrDefaultAsync();

            if (materialData == null) {
                return NotFound(new {
                    error = "WarehouseItem not found "
                });
            }

            //Thread.Sleep(10000);
            return Ok(materialData);
        }

        [HttpGet("FiscalPeriod")]
        public async Task<IActionResult> GetFiscalPeriod(DateTime forDate) {
            //Debug.Print("******Inside GetFiscal period " + forDate.ToString());
            var dateOfTrans = forDate;
            var fiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(p =>
                dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
            if (fiscalPeriod == null) {
                Debug.Print("******Inside GetFiscal period No Fiscal Period found");
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                return NotFound(new {
                    error = "No fiscal period includes date"
                });
            }

            Debug.Print("******Inside GetFiscal period Returning period id " + fiscalPeriod.Id);
            return Ok(new { PeriodId = fiscalPeriod.Id });
        }

        [HttpGet("SalesSeriesData")]
        public async Task<IActionResult> GetSalesSeriesData(int seriesId) {
            Debug.Print("Inside GetSalesSeriesData " + seriesId.ToString());
            var salesSeriesDef = await _context.SellDocSeriesDefs.SingleOrDefaultAsync(p => p.Id == seriesId);
            if (salesSeriesDef == null) {
                Debug.Print("Inside GetSalesSeriesData No Series found");
                return NotFound(new {
                    error = "No Series Found"
                });
            }

            await _context.Entry(salesSeriesDef)
                .Reference(p => p.SellDocTypeDef)
                .LoadAsync();
            var salesTypeDef = salesSeriesDef.SellDocTypeDef;
            var usedPrice = salesTypeDef.UsedPrice;

            Debug.Print("Inside GetSalesSeriesData Returning usedPrice " + usedPrice.ToString());
            return Ok(new { UsedPrice = usedPrice });
        }

        [HttpGet("RecSeriesData")]
        public async Task<IActionResult> GetRecSeriesData(int seriesId, RecurringDocTypeEnum docType) {
            switch (docType) {
                case RecurringDocTypeEnum.BuyType:
                    var buySeriesDef = await _context.BuyDocSeriesDefs.SingleOrDefaultAsync(p => p.Id == seriesId);
                    if (buySeriesDef == null) {
                        Debug.Print("Inside GetBuySeriesData No Series found");
                        return NotFound(new {
                            error = "No Series Found"
                        });
                    }

                    //BuySeriesInSession(seriesId.ToString());
                    await _context.Entry(buySeriesDef)
                        .Reference(p => p.BuyDocTypeDef)
                        .LoadAsync();
                    var buyTypeDef = buySeriesDef.BuyDocTypeDef;
                    var usedBuyPrice = buyTypeDef.UsedPrice;
                    Debug.Print("Inside GetBuySeriesData Returning usedPrice " + usedBuyPrice.ToString());
                    return Ok(new { UsedPrice = usedBuyPrice });
                //break;
                case RecurringDocTypeEnum.SellType:
                    Debug.Print("Inside GetSalesSeriesData " + seriesId.ToString());
                    var salesSeriesDef = await _context.SellDocSeriesDefs.SingleOrDefaultAsync(p => p.Id == seriesId);
                    if (salesSeriesDef == null) {
                        Debug.Print("Inside GetSalesSeriesData No Series found");
                        return NotFound(new {
                            error = "No Series Found"
                        });
                    }

                    await _context.Entry(salesSeriesDef)
                        .Reference(p => p.SellDocTypeDef)
                        .LoadAsync();
                    var salesTypeDef = salesSeriesDef.SellDocTypeDef;
                    var usedSellPrice = salesTypeDef.UsedPrice;
                    Debug.Print("Inside GetSalesSeriesData Returning usedPrice " + usedSellPrice.ToString());
                    return Ok(new { UsedPrice = usedSellPrice });
                //break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(docType), docType, null);
            }
        }

        [HttpGet("BuySeriesData")]
        public async Task<IActionResult> GetBuySeriesData(int seriesId) {
            Debug.Print("Inside GetBuySeriesData " + seriesId.ToString());
            var buySeriesDef = await _context.BuyDocSeriesDefs.SingleOrDefaultAsync(p => p.Id == seriesId);
            if (buySeriesDef == null) {
                Debug.Print("Inside GetBuySeriesData No Series found");
                return NotFound(new {
                    error = "No Series Found"
                });
            }

            //BuySeriesInSession(seriesId.ToString());
            await _context.Entry(buySeriesDef)
                .Reference(p => p.BuyDocTypeDef)
                .LoadAsync();
            var buyTypeDef = buySeriesDef.BuyDocTypeDef;
            var usedPrice = buyTypeDef.UsedPrice;

            Debug.Print("Inside GetBuySeriesData Returning usedPrice " + usedPrice.ToString());
            return Ok(new { UsedPrice = usedPrice });
        }

        [HttpGet("WarehouseTransType")]
        public async Task<IActionResult> GetWarehouseTransType(int seriesId) {
            Debug.Print("******Inside GetWarehouseTransType for series ID " + seriesId.ToString());

            var transWarehouseDocSeriesDef = await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                p.Id == seriesId);
            if (transWarehouseDocSeriesDef == null) {
                Debug.Print("******Inside GetWarehouseTransType No Series found");
                ModelState.AddModelError(string.Empty, "No Series found");
                return NotFound(new {
                    error = "No Series found"
                });
            }

            await _context.Entry(transWarehouseDocSeriesDef).Reference(t => t.TransWarehouseDocTypeDef).LoadAsync();
            var transWarehouseDocTypeDef = transWarehouseDocSeriesDef.TransWarehouseDocTypeDef;
            await _context.Entry(transWarehouseDocTypeDef).Reference(t => t.TransWarehouseDef).LoadAsync();
            var transWarehouseDef = transWarehouseDocTypeDef.TransWarehouseDef;
            var inventoryActionType = transWarehouseDef.MaterialInventoryAction;
            string transType = "";
            switch (inventoryActionType) {
                case InventoryActionEnum.InventoryActionEnumNoChange:
                    transType = "WarehouseTransactionTypeIgnore";
                    break;
                case InventoryActionEnum.InventoryActionEnumImport:
                    transType = "WarehouseTransactionTypeImport";
                    break;
                case InventoryActionEnum.InventoryActionEnumExport:
                    transType = "WarehouseTransactionTypeExport";
                    break;
                case InventoryActionEnum.InventoryActionEnumNegativeImport:
                    transType = "WarehouseTransactionTypeImport";
                    break;
                case InventoryActionEnum.InventoryActionEnumNegativeExport:
                    transType = "WarehouseTransactionTypeExport";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Print("******Inside GetWarehouseTransType Returning Action value " + transType);
            return Ok(new { TransType = transType });
        }

        [HttpPost("CreateRecurringDoc")]
        public async Task<IActionResult> CreateRecurringDoc([FromBody] RecurringTransDocCreateAjaxDto data) {
            const string defaultBuySectionCode = "SYS-BUY-MATERIALS-SCN";
            const string defaultSellSectionCode = "SYS-SELL-COMBINED-SCN";
            // bool noSupplierTrans = false;
            //bool noWarehouseTrans = false;


            RecurringDocCreateAjaxNoLinesDto transToAttachNoLines;
            RecurringTransDoc transToAttach;
            DateTime dateOfTrans;

            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<RecurringDocCreateAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<RecurringTransDoc>(transToAttachNoLines);
                dateOfTrans = data.NextTransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            using (var transaction = _context.Database.BeginTransaction()) {
                int sectionId = 0;
                switch (transToAttach.RecurringDocType) {
                    case RecurringDocTypeEnum.BuyType:
                        var buySeries =
                            await _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.DocSeriesId);
                        if (buySeries is null) {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                            return NotFound(new {
                                error = "Buy Doc Series not found"
                            });
                        }

                        await _context.Entry(buySeries).Reference(t => t.BuyDocTypeDef).LoadAsync();
                        var buyTypeDef = buySeries.BuyDocTypeDef;
                        if (buyTypeDef == null) {
                            transaction.Rollback();
                            return NotFound(new {
                                error = "Doc Series has not doc type definition"
                            });
                        }

                        #region Section Management

                        if (buyTypeDef.SectionId == 0) {
                            var section =
                                await _context.Sections.SingleOrDefaultAsync(s =>
                                    s.SystemName == defaultBuySectionCode);
                            if (section == null) {
                                transaction.Rollback();
                                ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                                return NotFound(new {
                                    error = "Could not locate section "
                                });
                            }

                            sectionId = section.Id;
                        }
                        else {
                            sectionId = buyTypeDef.SectionId;
                        }

                        #endregion

                        break;
                    case RecurringDocTypeEnum.SellType:
                        var sellSeries =
                            await _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.DocSeriesId);
                        if (sellSeries is null) {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                            return NotFound(new {
                                error = "Sell Doc Series not found"
                            });
                        }

                        await _context.Entry(sellSeries).Reference(t => t.SellDocTypeDef).LoadAsync();
                        var sellTypeDef = sellSeries.SellDocTypeDef;
                        if (sellTypeDef == null) {
                            transaction.Rollback();
                            return NotFound(new {
                                error = "Doc Series has not doc type definition"
                            });
                        }

                        #region Section Management

                        if (sellTypeDef.SectionId == 0) {
                            var section =
                                await _context.Sections.SingleOrDefaultAsync(
                                    s => s.SystemName == defaultSellSectionCode);
                            if (section == null) {
                                transaction.Rollback();
                                ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                                return NotFound(new {
                                    error = "Could not locate section "
                                });
                            }

                            sectionId = section.Id;
                        }
                        else {
                            sectionId = sellTypeDef.SectionId;
                        }

                        #endregion

                        break;
                    default:
                        break;
                }

                transToAttach.SectionId = sectionId;
                transToAttach.DocTypeId = data.DocSeriesId;
                _context.RecurringTransDocs.Add(transToAttach);

                try {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }

                var docId = _context.Entry(transToAttach).Entity.Id;

                foreach (var dataBuyDocLine in data.DocLines) {
                    var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    var docLine = new RecurringTransDocLine();
                    decimal unitPrice = dataBuyDocLine.Price;
                    decimal units = (decimal)dataBuyDocLine.Q1;
                    decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                    decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    docLine.UnitPrice = unitPrice;
                    docLine.AmountFpa = lineFpaAmount;
                    docLine.AmountNet = lineNetAmount;
                    docLine.AmountDiscount = lineDiscountAmount;
                    docLine.DiscountRate = discountRate;
                    docLine.FpaRate = fpaRate;
                    docLine.WarehouseItemId = dataBuyDocLine.WarehouseItemId;
                    docLine.Quontity1 = dataBuyDocLine.Q1;
                    docLine.Quontity2 = dataBuyDocLine.Q2;
                    docLine.PrimaryUnitId = dataBuyDocLine.MainUnitId;
                    docLine.SecondaryUnitId = dataBuyDocLine.SecUnitId;
                    docLine.Factor = dataBuyDocLine.Factor;
                    docLine.RecurringTransDocId = docId;
                    docLine.Etiology = transToAttach.Etiology;
                    //_context.Entry(transToAttach).Entity
                    transToAttach.DocLines.Add(docLine);
                }

                try {
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }

        [HttpPost("UpdateRecurringDoc")]
        public async Task<IActionResult> UpdateRecurringDoc([FromBody] RecurringTransDocModifyAjaxDto data) {
            const string defaultBuySectionCode = "SYS-BUY-MATERIALS-SCN";
            const string defaultSellSectionCode = "SYS-SELL-COMBINED-SCN";
            RecurringTransDocModifyAjaxNoLinesDto transToAttachNoLines;
            RecurringTransDoc transToAttach;
            DateTime dateOfTrans;
            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<RecurringTransDocModifyAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<RecurringTransDoc>(transToAttachNoLines);
                dateOfTrans = data.NextTransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            using (var transaction = _context.Database.BeginTransaction()) {
                _context.RecurringTransDocLines.RemoveRange(
                    _context.RecurringTransDocLines.Where(p => p.RecurringTransDocId == data.Id));
                int sectionId = 0;
                switch (transToAttach.RecurringDocType) {
                    case RecurringDocTypeEnum.BuyType:
                        var buySeries =
                            await _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.DocSeriesId);
                        if (buySeries is null) {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                            return NotFound(new {
                                error = "Buy Doc Series not found"
                            });
                        }

                        await _context.Entry(buySeries).Reference(t => t.BuyDocTypeDef).LoadAsync();
                        var buyTypeDef = buySeries.BuyDocTypeDef;
                        if (buyTypeDef == null) {
                            transaction.Rollback();
                            return NotFound(new {
                                error = "Doc Series has not doc type definition"
                            });
                        }

                        #region Section Management

                        if (buyTypeDef.SectionId == 0) {
                            var section =
                                await _context.Sections.SingleOrDefaultAsync(s =>
                                    s.SystemName == defaultBuySectionCode);
                            if (section == null) {
                                transaction.Rollback();
                                ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                                return NotFound(new {
                                    error = "Could not locate section "
                                });
                            }

                            sectionId = section.Id;
                        }
                        else {
                            sectionId = buyTypeDef.SectionId;
                        }

                        #endregion

                        break;
                    case RecurringDocTypeEnum.SellType:
                        var sellSeries =
                            await _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.DocSeriesId);
                        if (sellSeries is null) {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                            return NotFound(new {
                                error = "Sell Doc Series not found"
                            });
                        }

                        await _context.Entry(sellSeries).Reference(t => t.SellDocTypeDef).LoadAsync();
                        var sellTypeDef = sellSeries.SellDocTypeDef;
                        if (sellTypeDef == null) {
                            transaction.Rollback();
                            return NotFound(new {
                                error = "Doc Series has not doc type definition"
                            });
                        }

                        #region Section Management

                        if (sellTypeDef.SectionId == 0) {
                            var section =
                                await _context.Sections.SingleOrDefaultAsync(
                                    s => s.SystemName == defaultSellSectionCode);
                            if (section == null) {
                                transaction.Rollback();
                                ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                                return NotFound(new {
                                    error = "Could not locate section "
                                });
                            }

                            sectionId = section.Id;
                        }
                        else {
                            sectionId = sellTypeDef.SectionId;
                        }

                        #endregion

                        break;
                    default:
                        break;
                }

                transToAttach.SectionId = sectionId;
                transToAttach.DocTypeId = data.DocSeriesId;

                _context.Entry(transToAttach).State = EntityState.Modified;
                var docId = transToAttach.Id;
                //--------------------------------------

                foreach (var dataBuyDocLine in data.BuyDocLines) {
                    var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    var docLine = new RecurringTransDocLine();
                    decimal unitPrice = dataBuyDocLine.Price;
                    decimal units = (decimal)dataBuyDocLine.Q1;
                    decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                    decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    docLine.UnitPrice = unitPrice;
                    docLine.AmountFpa = lineFpaAmount;
                    docLine.AmountNet = lineNetAmount;
                    docLine.AmountDiscount = lineDiscountAmount;
                    docLine.DiscountRate = discountRate;
                    docLine.FpaRate = fpaRate;
                    docLine.WarehouseItemId = dataBuyDocLine.WarehouseItemId;
                    docLine.Quontity1 = dataBuyDocLine.Q1;
                    docLine.Quontity2 = dataBuyDocLine.Q2;
                    docLine.PrimaryUnitId = dataBuyDocLine.MainUnitId;
                    docLine.SecondaryUnitId = dataBuyDocLine.SecUnitId;
                    docLine.Factor = dataBuyDocLine.Factor;
                    docLine.RecurringTransDocId = docId;
                    docLine.Etiology = transToAttach.Etiology;
                    //_context.Entry(transToAttach).Entity

                    try {
                        transToAttach.DocLines.Add(docLine);
                    }
                    catch (Exception e) {
                        transaction.Rollback();
                        string msg = e.InnerException.Message;
                        return BadRequest(new {
                            error = e.Message + " " + msg
                        });
                    }

                    #endregion
                }

                try {
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e) {
                    transaction.Rollback();
                    string msg = e.InnerException.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }

        [HttpPost("AddBuyPaymentMapping")]
        public async Task<IActionResult> PostBuyPaymentMapping([FromBody] IdList docIds) {

            if (docIds == null) {
                return BadRequest(new {
                    ErrorMessage = "Empty request data"
                });
            }

            var docId = docIds.Ids[0];
            var doc = await _context.BuyDocuments.FindAsync(docId);
            if (doc == null) {
                return BadRequest(new {
                    ErrorMessage = "Requested document not found"
                });
            }
            await _context.Entry(doc)
                .Reference(t => t.BuyDocSeries)
                .LoadAsync();
            var docSeries = doc.BuyDocSeries;
            if (docSeries.PayoffSeriesId == null || docSeries.PayoffSeriesId == 0) {
                return BadRequest(new {
                    ErrorMessage = "No payoff series defined"
                });
            }
            int payoffSeriesId = (int)docSeries.PayoffSeriesId;
            var payoffSeries = await _context.TransTransactorDocSeriesDefs.FindAsync(payoffSeriesId);

            if (payoffSeries == null) {
                return BadRequest(new {
                    ErrorMessage = "Payoff series definition not found"
                });
            }
            await _context.Entry(payoffSeries).Reference(t => t.TransTransactorDocTypeDef)
                .LoadAsync();
            var payoffSeriesType = payoffSeries.TransTransactorDocTypeDef;
            if (payoffSeriesType == null) {
                return BadRequest(new {
                    ErrorMessage = "Payoff series type Definition not found"
                });
            }
            await _context.Entry(payoffSeriesType)
                .Reference(t => t.TransTransactorDef)
                .LoadAsync();
            var payoffTransactorTransactionDef = payoffSeriesType.TransTransactorDef;
            #region Section Management
            var scnId = payoffSeriesType.SectionId == 0 ? doc.SectionId : payoffSeriesType.SectionId;
            #endregion
            var payoffTransaction = new TransactorTransaction {
                TransDate = DateTime.Today,
                TransTransactorDocSeriesId = payoffSeriesId,
                TransTransactorDocTypeId = payoffSeries.TransTransactorDocTypeDefId,
                TransRefCode = doc.TransRefCode,
                TransactorId = doc.TransactorId,
                SectionId = scnId,
                CreatorId = 0,
                CreatorSectionId = 0,
                FiscalPeriodId = doc.FiscalPeriodId,
                AmountFpa = doc.AmountFpa,
                AmountNet = doc.AmountNet,
                AmountDiscount = doc.AmountDiscount,
                Etiology = $"Εξοφληση παραστατικού με αριθμό {doc.TransRefCode}",
                CompanyId = doc.CompanyId
            };
            ActionHandlers.TransactorFinAction(payoffTransactorTransactionDef.FinancialTransAction, payoffTransaction);
            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {
                    await _context.TransactorTransactions.AddAsync(payoffTransaction);
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting transactor transaction {e.Message}"
                    });
                }

                var mapping = new BuyDocTransPaymentMapping {
                    BuyDocument = doc,
                    TransactorTransaction = payoffTransaction,
                    AmountUsed = payoffTransaction.AmountNet + payoffTransaction.AmountFpa -
                                 payoffTransaction.AmountDiscount
                };
                try {
                    await _context.BuyDocTransPaymentMappings.AddAsync(mapping);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting buy payment mapping {e.Message}"
                    });
                }

                try {
                    var recs = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    // var ms = new StringBuilder()
                    //     .Append("Η αντιστοίχιση εικόνων ολοκληρώθηκε")
                    //     .Append($"</br>Στάλθηκαν:       {requested} εικόνες")
                    //     .Append($"</br>Αντιστοιχισμένες:{allreadyAssigned} εικόνες")
                    //     .Append($"</br>Επιτυχής :       {addedCount} εικόνες");
                    //
                    // string message = ms.ToString();
                    return Ok(new { Message = $"Successfully added mappings" });
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error updating database {e.Message}"
                    });
                }
            }


        }
        [HttpPost("AddSalePaymentMapping")]
        public async Task<IActionResult> PostSalePaymentMapping([FromBody] IdList docIds) {

            if (docIds == null) {
                return BadRequest(new {
                    ErrorMessage = "Empty request data"
                });
            }

            var docId = docIds.Ids[0];
            var doc = await _context.SellDocuments.FindAsync(docId);
            if (doc == null) {
                return BadRequest(new {
                    ErrorMessage = "Requested document not found"
                });
            }
            await _context.Entry(doc)
                .Reference(t => t.SellDocSeries)
                .LoadAsync();
            var docSeries = doc.SellDocSeries;
            if (docSeries.PayoffSeriesId == null || docSeries.PayoffSeriesId == 0) {
                return BadRequest(new {
                    ErrorMessage = "No payoff series defined"
                });
            }
            int payoffSeriesId = (int)docSeries.PayoffSeriesId;
            var payoffSeries = await _context.TransTransactorDocSeriesDefs.FindAsync(payoffSeriesId);

            if (payoffSeries == null) {
                return BadRequest(new {
                    ErrorMessage = "Payoff series definition not found"
                });
            }
            await _context.Entry(payoffSeries).Reference(t => t.TransTransactorDocTypeDef)
                .LoadAsync();
            var payoffSeriesType = payoffSeries.TransTransactorDocTypeDef;
            if (payoffSeriesType == null) {
                return BadRequest(new {
                    ErrorMessage = "Payoff series type Definition not found"
                });
            }
            await _context.Entry(payoffSeriesType)
                .Reference(t => t.TransTransactorDef)
                .LoadAsync();
            var payoffTransactorTransactionDef = payoffSeriesType.TransTransactorDef;
            #region Section Management
            var scnId = payoffSeriesType.SectionId == 0 ? doc.SectionId : payoffSeriesType.SectionId;
            #endregion
            var payoffTransaction = new TransactorTransaction {
                TransDate = DateTime.Today,
                TransTransactorDocSeriesId = payoffSeriesId,
                TransTransactorDocTypeId = payoffSeries.TransTransactorDocTypeDefId,
                TransRefCode = doc.TransRefCode,
                TransactorId = doc.TransactorId,
                SectionId = scnId,
                CreatorId = 0,
                CreatorSectionId = 0,
                FiscalPeriodId = doc.FiscalPeriodId,
                AmountFpa = doc.AmountFpa,
                AmountNet = doc.AmountNet,
                AmountDiscount = doc.AmountDiscount,
                Etiology = $"Εξοφληση παραστατικού με αριθμό {doc.TransRefCode}",
                CompanyId = doc.CompanyId
            };
            ActionHandlers.TransactorFinAction(payoffTransactorTransactionDef.FinancialTransAction, payoffTransaction);
            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {
                    await _context.TransactorTransactions.AddAsync(payoffTransaction);
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting transactor transaction {e.Message}"
                    });
                }

                var mapping = new SellDocTransPaymentMapping() {
                    SellDocument = doc,
                    TransactorTransaction = payoffTransaction,
                    AmountUsed = payoffTransaction.AmountNet + payoffTransaction.AmountFpa -
                                 payoffTransaction.AmountDiscount
                };
                try {
                    await _context.SellDocTransPaymentMappings.AddAsync(mapping);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting buy payment mapping {e.Message}"
                    });
                }

                try {
                    var recs = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    // var ms = new StringBuilder()
                    //     .Append("Η αντιστοίχιση εικόνων ολοκληρώθηκε")
                    //     .Append($"</br>Στάλθηκαν:       {requested} εικόνες")
                    //     .Append($"</br>Αντιστοιχισμένες:{allreadyAssigned} εικόνες")
                    //     .Append($"</br>Επιτυχής :       {addedCount} εικόνες");
                    //
                    // string message = ms.ToString();
                    return Ok(new { Message = $"Successfully added mappings" });
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error updating database {e.Message}"
                    });
                }
            }


        }
        [HttpPost("AddBuyPaymentMappingList")]
        public async Task<IActionResult> AddBuyPaymentMappingList([FromBody] PaymentMappingCreateDto data) {
            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            if (data.DocId <= 0) {
                return BadRequest(new {
                    error = "Document Id Out of range"
                });
            }

            var doc = await _context.BuyDocuments.FindAsync(data.DocId);
            if (doc == null) {
                return BadRequest(new {
                    error = "Document not found"
                });
            }
            await _context.Entry(doc)
                .Reference(t => t.Company)
                .LoadAsync();
            var companyCurrencyId = doc.Company.CurrencyId;
            var currencyRates = await _context.ExchangeRates.OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var paymentMapping in data.PaymentMappingLines) {
                if (companyCurrencyId != data.DisplayCurrencyId) {
                    if (data.DisplayCurrencyId != 1) {
                        var r = currencyRates.Where(p => p.CurrencyId == data.DisplayCurrencyId)
                            .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                        if (r != null) {
                            paymentMapping.AmountUsed /= r.Rate;
                        }

                    }
                    else {
                        var r = currencyRates.Where(p => p.CurrencyId == companyCurrencyId)
                            .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                        if (r != null) {
                            paymentMapping.AmountUsed *= r.Rate;

                        }
                    }
                }

                var mapping = new BuyDocTransPaymentMapping {
                    BuyDocumentId = data.DocId,
                    TransactorTransactionId = paymentMapping.ReceiptId,
                    AmountUsed = paymentMapping.AmountUsed
                };
                try {
                    await _context.BuyDocTransPaymentMappings.AddAsync(mapping);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting buy payment mapping {e.Message}"
                    });
                }
            }


            try {
                var recs = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = $"Successfully added {recs} mappings" });
            }
            catch (Exception e) {
                Console.WriteLine(e);
                await transaction.RollbackAsync();
                return BadRequest(new {
                    ErrorMessage = $"Error updating database {e.Message}"
                });
            }
        }
        [HttpPost("AddSalePaymentMappingList")]
        public async Task<IActionResult> AddSalePaymentMappingList([FromBody] PaymentMappingCreateDto data) {
            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }
            if (data.DocId <= 0) {
                return BadRequest(new {
                    error = "Document Id Out of range"
                });
            }

            var doc = await _context.SellDocuments.FindAsync(data.DocId);
            if (doc == null) {
                return BadRequest(new {
                    error = "Document not found"
                });
            }
            await _context.Entry(doc)
                .Reference(t => t.Company)
                .LoadAsync();
            var companyCurrencyId = doc.Company.CurrencyId;
            var currencyRates = await _context.ExchangeRates.OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var paymentMapping in data.PaymentMappingLines) {
                if (companyCurrencyId != data.DisplayCurrencyId) {
                    if (data.DisplayCurrencyId != 1) {
                        var r = currencyRates.Where(p => p.CurrencyId == data.DisplayCurrencyId)
                            .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                        if (r != null) {
                            paymentMapping.AmountUsed /= r.Rate;
                        }

                    }
                    else {
                        var r = currencyRates.Where(p => p.CurrencyId == companyCurrencyId)
                            .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                        if (r != null) {
                            paymentMapping.AmountUsed *= r.Rate;

                        }
                    }
                }
                var mapping = new SellDocTransPaymentMapping() {
                    SellDocumentId = data.DocId,
                    TransactorTransactionId = paymentMapping.ReceiptId,
                    AmountUsed = paymentMapping.AmountUsed
                };
                try {
                    await _context.SellDocTransPaymentMappings.AddAsync(mapping);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    return BadRequest(new {
                        ErrorMessage = $"Error inserting buy payment mapping {e.Message}"
                    });
                }
            }


            try {
                var recs = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = $"Successfully added {recs} mappings" });
            }
            catch (Exception e) {
                Console.WriteLine(e);
                await transaction.RollbackAsync();
                return BadRequest(new {
                    ErrorMessage = $"Error updating database {e.Message}"
                });
            }
        }
        [HttpPost("MaterialBuyDoc")]
        public async Task<IActionResult> PostMaterialBuyDoc([FromBody] BuyDocCreateAjaxDto data) {
            const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            // bool noSupplierTrans = false;
            bool noWarehouseTrans = false;


            BuyDocCreateAjaxNoLinesDto transToAttachNoLines;
            BuyDocument transToAttach;
            DateTime dateOfTrans;

            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<BuyDocCreateAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<BuyDocument>(transToAttachNoLines);
                dateOfTrans = data.TransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            //var tr = _context.Database.CurrentTransaction;
            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.BuyDocSeriesId);

                if (docSeries is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new {
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
                if (docTypeDef.SectionId == 0) {
                    var sectn = await _context.Sections.AsNoTracking().SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else {
                    sectionId = docTypeDef.SectionId;
                }

                #endregion

                //var transSupplierDef = docTypeDef.TransSupplierDef;
                var transTransactorDef = docTypeDef.TransTransactorDef;
                var transWarehouseDef = docTypeDef.TransWarehouseDef;

                transToAttach.SectionId = sectionId;
                transToAttach.FiscalPeriodId = fiscalPeriod.Id;
                transToAttach.BuyDocTypeId = docSeries.BuyDocTypeDefId;
                await _context.BuyDocuments.AddAsync(transToAttach);

                try {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }

                var docId = _context.Entry(transToAttach).Entity.Id;


                if (transTransactorDef.DefaultDocSeriesId > 0) {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                    sTransactorTransaction.TransactorId = data.TransactorId;
                    sTransactorTransaction.SectionId = sectionId;
                    sTransactorTransaction.CfAccountId = 0;
                    sTransactorTransaction.CreatorSectionId = sectionId;
                    sTransactorTransaction.TransTransactorDocTypeId =
                        transTransactorDefaultSeries.TransTransactorDocTypeDefId;
                    sTransactorTransaction.TransTransactorDocSeriesId = transTransactorDefaultSeries.Id;
                    sTransactorTransaction.FiscalPeriodId = fiscalPeriod.Id;
                    sTransactorTransaction.CreatorId = docId;
                    ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, sTransactorTransaction);

                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    try {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return BadRequest(new {
                            error = e.Message + " " + msg
                        });
                    }
                }

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto) {
                    var autoPaySeriesId = transToAttach.BuyDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0) {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null) {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new {
                                error = "AutoPayOff series not found"
                            });
                        }
                        var transactor = await _context.Transactors
                            .Where(p => p.Id == data.TransactorId)
                           .SingleOrDefaultAsync();
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";
                        sTransactorTransaction.TransactorId = data.TransactorId;

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
                        if (transTransactorDocTypeDef.SectionId == 0) {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else {
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
                        try {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e) {
                            Console.WriteLine(e);
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0) {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0) {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null) {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null) {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";



                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction {
                                            TransDate = data.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = data.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = data.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet - sTransactorTransaction.AmountDiscount + sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e) {
                                            await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return BadRequest(new {
                                                error = e.Message + " " + msg
                                            });
                                        }


                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new BuyDocTransPaymentMapping() {
                                BuyDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                             sTransactorTransaction.AmountDiscount
                            };
                            await _context.BuyDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }

                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0) {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else {
                    noWarehouseTrans = true;
                }

                foreach (var dataBuyDocLine in data.BuyDocLines) {
                    var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                    var material = await _context.WarehouseItems
                        .SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
                            error = "Could not locate material in Doc Line "
                        });
                    }

                    #region MaterialLine

                    
                    var transUnitId = dataBuyDocLine.TransactionUnitId;
                    var transUnitFactor = dataBuyDocLine.TransactionUnitFactor;
                    // var factor = dataBuyDocLine.Factor;
                    decimal transPrice = dataBuyDocLine.TransUnitPrice;
                    double transUnits = dataBuyDocLine.TransactionQuantity;
                    decimal units = (decimal)dataBuyDocLine.Q1;
                    decimal unitPrice = dataBuyDocLine.Price;
                    decimal fpaRate = (decimal)dataBuyDocLine.FpaRate;
                    decimal discountRate = (decimal)dataBuyDocLine.DiscountRate;
                    decimal lineNetAmount = unitPrice * units;
                    decimal lineDiscountAmount = lineNetAmount * discountRate;
                    decimal lineFpaAmount = (lineNetAmount - lineDiscountAmount) * fpaRate;
                    var buyMaterialLine = new BuyDocLine {
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
                        TransactionUnitFactor = transUnitFactor
                    };
                    //_context.Entry(transToAttach).Entity
                    transToAttach.BuyDocLines.Add(buyMaterialLine);

                    #endregion

                    if (!noWarehouseTrans) {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction {
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

                try {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }


        [HttpPost("MaterialBuyDocUpdate")]
        public async Task<IActionResult> PutMaterialBuyDoc([FromBody] BuyDocModifyAjaxDto data) {
            const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            bool noWarehouseTrans;

            BuyDocModifyAjaxNoLinesDto transToAttachNoLines;
            BuyDocument transToAttach;
            DateTime dateOfTrans;
            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<BuyDocModifyAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<BuyDocument>(transToAttachNoLines);
                dateOfTrans = data.TransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                _context.BuyDocLines.RemoveRange(_context.BuyDocLines.Where(p => p.BuyDocumentId == data.Id));
                _context.TransactorTransactions.RemoveRange(
                    _context.TransactorTransactions.Where(p =>
                        p.CreatorSectionId == data.SectionId && p.CreatorId == data.Id));
                _context.WarehouseTransactions.RemoveRange(
                    _context.WarehouseTransactions.Where(p => p.SectionId == data.SectionId && p.CreatorId == data.Id));
                _context.BuyDocTransPaymentMappings.RemoveRange(
                    _context.BuyDocTransPaymentMappings.Where(p => p.BuyDocumentId == data.Id));
                _context.CashFlowAccountTransactions.RemoveRange(
                    _context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == data.SectionId && p.CreatorId == data.Id));
                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.BuyDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.BuyDocSeriesId);

                if (docSeries is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new {
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
                if (docTypeDef.SectionId == 0) {
                    var sectn = await _context.Sections.AsNoTracking().SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else {
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
                if (transTransactorDef.DefaultDocSeriesId > 0) {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    try {
                        var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(data);
                        //Ετσι δεν μεταφέρει το Id απο το data
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);

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

                        await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    }
                    catch (Exception e) {
                        await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return BadRequest(new {
                            error = e.Message + " " + msg
                        });
                    }
                }

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto) {
                    var autoPaySeriesId = transToAttach.BuyDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0) {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null) {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new {
                                error = "AutoPayOff series not found"
                            });
                        }
                        var transactor = await _context.Transactors
                            .Where(p => p.Id == data.TransactorId)
                            .SingleOrDefaultAsync();
                        var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(data);
                        //Ετσι δεν μεταφέρει το Id απο το data
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";
                        sTransactorTransaction.TransactorId = data.TransactorId;
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
                        if (transTransactorDocTypeDef.SectionId == 0) {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else {
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
                        try {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0) {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0) {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null) {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null) {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";



                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction {
                                            TransDate = data.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = data.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = data.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet - sTransactorTransaction.AmountDiscount + sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e) {
                                            await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return BadRequest(new {
                                                error = e.Message + " " + msg
                                            });
                                        }


                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new BuyDocTransPaymentMapping() {
                                BuyDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.TransNetAmount + sTransactorTransaction.TransFpaAmount -
                                             sTransactorTransaction.TransDiscountAmount
                            };
                            await _context.BuyDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }


                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0) {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else {
                    noWarehouseTrans = true;
                }

                foreach (var dataBuyDocLine in data.BuyDocLines) {
                    var warehouseItemId = dataBuyDocLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
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
                    var warehouseItemLine = new BuyDocLine {
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
                        TransactionUnitFactor = transUnitFactor
                    };

                    //_context.Entry(transToAttach).Entity

                    try {
                        transToAttach.BuyDocLines.Add(warehouseItemLine);
                    }
                    catch (Exception e) {
                        await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return BadRequest(new {
                            error = e.Message + " " + msg
                        });
                    }

                    #endregion

                    if (!noWarehouseTrans) {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction {
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

                        try {
                            await _context.WarehouseTransactions.AddAsync(warehouseTrans);
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }

                        #endregion
                    }
                }


                try {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e) {
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }


        [HttpPost("SalesDoc")]
        public async Task<IActionResult> PostSalesDoc([FromBody] SellDocCreateAjaxDto data) {
            const string sectionCode = "SYS-SELL-COMBINED-SCN";

            // var sessionCompanyId = HttpContext.Session.GetString("CompanyId");

            // bool noSupplierTrans = false;
            bool noWarehouseTrans = false;

            SellDocCreateAjaxNoLinesDto transToAttachNoLines;
            SellDocument transToAttach;
            DateTime dateOfTrans;

            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<SellDocCreateAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<SellDocument>(transToAttachNoLines);
                dateOfTrans = data.TransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.SellDocSeriesId);

                if (docSeries is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new {
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
                if (docTypeDef.SectionId == 0) {
                    var sectn = await _context.Sections.AsNoTracking().SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else {
                    sectionId = docTypeDef.SectionId;
                }

                #endregion

                var transTransactorDef = docTypeDef.TransTransactorDef;
                var transWarehouseDef = docTypeDef.TransWarehouseDef;

                transToAttach.SectionId = sectionId;
                transToAttach.FiscalPeriodId = fiscalPeriod.Id;
                transToAttach.SellDocTypeId = docSeries.SellDocTypeDefId;
                await _context.SellDocuments.AddAsync(transToAttach);

                try {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }

                var docId = _context.Entry(transToAttach).Entity.Id;


                if (transTransactorDef.DefaultDocSeriesId > 0) {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new {
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

                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                    try {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e) {
                        await transaction.RollbackAsync();
                        string msg = e.InnerException?.Message;
                        return BadRequest(new {
                            error = e.Message + " " + msg
                        });
                    }
                }

                #region Αυτόματη Εξόφληση

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto) {
                    var autoPaySeriesId = transToAttach.SellDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0) {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null) {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new {
                                error = "AutoPayOff series not found"
                            });
                        }
                        var transactor = await _context.Transactors
                            .Where(p => p.Id == data.TransactorId)
                            .SingleOrDefaultAsync();
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(data);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";

                        sTransactorTransaction.TransactorId = data.TransactorId;

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
                        if (transTransactorDocTypeDef.SectionId == 0) {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else {
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
                        try {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0) {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0) {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null) {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null) {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";



                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction {
                                            TransDate = data.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = data.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = data.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet - sTransactorTransaction.AmountDiscount + sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e) {
                                            await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return BadRequest(new {
                                                error = e.Message + " " + msg
                                            });
                                        }


                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new SellDocTransPaymentMapping() {
                                SellDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                             sTransactorTransaction.AmountDiscount
                            };
                            await _context.SellDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }

                #endregion

                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0) {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs
                            .FirstOrDefaultAsync(p =>
                            p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else {
                    noWarehouseTrans = true;
                }

                foreach (var docLine in data.SellDocLines) {
                    var warehouseItemId = docLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
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
                    var sellDocLine = new SellDocLine {
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
                        TransactionUnitFactor = transUnitFactor
                    };
                    //_context.Entry(transToAttach).Entity
                    transToAttach.SellDocLines.Add(sellDocLine);

                    #endregion

                    if (!noWarehouseTrans) {
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
                            UnitFactor = (decimal) docLine.Factor,
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

                try {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e) {
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }

        [HttpPost("SalesDocUpdate")]
        public async Task<IActionResult> PutSalesDoc([FromBody] SellDocModifyAjaxDto data) {
            const string sectionCode = "SYS-SELL-COMBINED-SCN";
            bool noWarehouseTrans;

            SellDocModifyAjaxNoLinesDto transToAttachNoLines;
            SellDocument transToAttach;
            DateTime dateOfTrans;
            if (data == null) {
                return BadRequest(new {
                    error = "Empty request data"
                });
            }

            try {
                transToAttachNoLines = _mapper.Map<SellDocModifyAjaxNoLinesDto>(data);
                transToAttach = _mapper.Map<SellDocument>(transToAttachNoLines);
                dateOfTrans = data.TransDate;
            }
            catch (Exception e) {
                return BadRequest(new {
                    error = e.Message
                });
            }

            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                _context.SellDocLines.RemoveRange(_context.SellDocLines.Where(p => p.SellDocumentId == data.Id));
                _context.TransactorTransactions.RemoveRange(
                    _context.TransactorTransactions.Where(p =>
                        p.CreatorSectionId == data.SectionId && p.CreatorId == data.Id));
                _context.WarehouseTransactions.RemoveRange(
                    _context.WarehouseTransactions.Where(p => p.SectionId == data.SectionId && p.CreatorId == data.Id));
                _context.SellDocTransPaymentMappings.RemoveRange(
                    _context.SellDocTransPaymentMappings.Where(p => p.SellDocumentId == data.Id));
                _context.CashFlowAccountTransactions.RemoveRange(
                    _context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == data.SectionId && p.CreatorId == data.Id));

                #region Fiscal Period

                var fiscalPeriod = await _context.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(p =>
                    dateOfTrans >= p.StartDate && dateOfTrans <= p.EndDate);
                if (fiscalPeriod == null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                    return NotFound(new {
                        error = "No Fiscal Period covers Transaction Date"
                    });
                }

                #endregion

                var docSeries = await
                    _context.SellDocSeriesDefs.SingleOrDefaultAsync(m => m.Id == data.SellDocSeriesId);

                if (docSeries is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    return NotFound(new {
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
                if (docTypeDef.SectionId == 0) {
                    var sectn = await _context.Sections.AsNoTracking().SingleOrDefaultAsync(s => s.SystemName == sectionCode);
                    if (sectn == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        return NotFound(new {
                            error = "Could not locate section "
                        });
                    }

                    sectionId = sectn.Id;
                }
                else {
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
                if (transTransactorDef.DefaultDocSeriesId > 0) {
                    var transTransactorDefaultSeries = await
                        _context.TransTransactorDocSeriesDefs.FirstOrDefaultAsync(p =>
                            p.Id == transTransactorDef.DefaultDocSeriesId);
                    if (transTransactorDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for transactor transaction not found");
                        return NotFound(new {
                            error = "Default series for transactor transaction not found"
                        });
                    }

                    var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(data);
                    //Ετσι δεν μεταφέρει το Id απο το data
                    var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);

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

                    await _context.TransactorTransactions.AddAsync(sTransactorTransaction);
                }

                //Αυτόματη εξόφληση
                var paymentMethod =
                    await _context.PaymentMethods
                        .FirstOrDefaultAsync(p => p.Id == transToAttach.PaymentMethodId);
                if (paymentMethod is null) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε ο τρόπος πληρωμής");
                    return NotFound(new {
                        error = "Δεν βρέθηκε ο τρόπος πληρωμής"
                    });
                }

                if (paymentMethod.AutoPayoffWay == SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto) {
                    var autoPaySeriesId = transToAttach.SellDocSeries.PayoffSeriesId;
                    var paymentCfAccountId = paymentMethod.CfAccountId;
                    if (autoPaySeriesId > 0) {
                        var transTransactorPayOffSeries = await
                            _context.TransTransactorDocSeriesDefs
                                .FirstOrDefaultAsync(p =>
                                p.Id == autoPaySeriesId);
                        if (transTransactorPayOffSeries == null) {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, "AutoPayOff series not found");
                            return NotFound(new {
                                error = "AutoPayOff series not found"
                            });
                        }
                        var transactor = await _context.Transactors
                            .Where(p => p.Id == data.TransactorId)
                            .SingleOrDefaultAsync();
                        var spTransactorCreateDto = _mapper.Map<TransactorTransCreateDto>(data);
                        var transTransactorEtiology =
                            $"{transTransactorPayOffSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";

                        //Ετσι δεν μεταφέρει το Id απο το data
                        var sTransactorTransaction = _mapper.Map<TransactorTransaction>(spTransactorCreateDto);
                        sTransactorTransaction.TransactorId = data.TransactorId;

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
                        if (transTransactorDocTypeDef.SectionId == 0) {
                            sTransactorTransaction.SectionId = sectionId;
                        }
                        else {
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
                        try {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                        //Cash Flow Account Transaction 
                        if (paymentCfAccountId > 0) {
                            var defaultCfaSeriesId = transTransactorPayOffSeries.DefaultCfaTransSeriesId;
                            if (defaultCfaSeriesId > 0) {
                                var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(defaultCfaSeriesId);
                                if (cfaSeries != null) {
                                    await _context.Entry(cfaSeries)
                                        .Reference(t => t.CashFlowDocTypeDefinition)
                                        .LoadAsync();

                                    var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                    if (cfaType != null) {
                                        await _context.Entry(cfaType)
                                            .Reference(t => t.CashFlowTransactionDefinition)
                                            .LoadAsync();

                                        var etiology =
                                            $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {data.Etiology} ";



                                        var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                        var cfaTrans = new CashFlowAccountTransaction {
                                            TransDate = data.TransDate,
                                            CashFlowAccountId = paymentCfAccountId,
                                            CompanyId = data.CompanyId,
                                            DocumentSeriesId = cfaSeries.Id,
                                            DocumentTypeId = cfaType.Id,
                                            Etiology = etiology,
                                            FiscalPeriodId = sTransactorTransaction.FiscalPeriodId,
                                            CreatorSectionId = sectionId,
                                            CreatorId = docId,
                                            RefCode = data.TransRefCode,
                                            Amount = sTransactorTransaction.AmountNet - sTransactorTransaction.AmountDiscount + sTransactorTransaction.AmountFpa,
                                            SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                        };
                                        ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                        await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                        sTransactorTransaction.CfAccountId = paymentCfAccountId;
                                        _context.Attach(sTransactorTransaction).State = EntityState.Modified;
                                        try {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e) {
                                            await transaction.RollbackAsync();
                                            string msg = e.InnerException?.Message;
                                            return BadRequest(new {
                                                error = e.Message + " " + msg
                                            });
                                        }


                                    }
                                }
                            }
                        }

                        //End Cash Flow Account Transaction 
                        try {
                            var payOfTransactionId = _context.Entry(sTransactorTransaction).Entity.Id;
                            var payOffMapping = new SellDocTransPaymentMapping() {
                                SellDocumentId = docId,
                                TransactorTransactionId = payOfTransactionId,
                                AmountUsed = sTransactorTransaction.AmountNet + sTransactorTransaction.AmountFpa -
                                             sTransactorTransaction.AmountDiscount
                            };
                            await _context.SellDocTransPaymentMappings.AddAsync(payOffMapping);
                        }
                        catch (Exception e) {
                            await transaction.RollbackAsync();
                            string msg = e.InnerException?.Message;
                            return BadRequest(new {
                                error = e.Message + " " + msg
                            });
                        }
                    }
                }

                int warehouseSeriesId = 0;
                int warehouseTypeId = 0;

                if (transWarehouseDef.DefaultDocSeriesId > 0) {
                    var transWarehouseDefaultSeries =
                        await _context.TransWarehouseDocSeriesDefs
                            .FirstOrDefaultAsync(p =>
                            p.Id == transWarehouseDef.DefaultDocSeriesId);
                    if (transWarehouseDefaultSeries == null) {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Default series for warehouse transaction not found");
                        return NotFound(new {
                            error = "Default series for warehouse transaction not found"
                        });
                    }

                    noWarehouseTrans = false;
                    warehouseSeriesId = transWarehouseDef.DefaultDocSeriesId;
                    warehouseTypeId = transWarehouseDefaultSeries.TransWarehouseDocTypeDefId;
                }
                else {
                    noWarehouseTrans = true;
                }

                foreach (var docLine in data.SellDocLines) {
                    var warehouseItemId = docLine.WarehouseItemId;
                    var material = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Id == warehouseItemId);
                    if (material is null) {
                        //Handle error
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Doc Line error null WarehouseItem");
                        return NotFound(new {
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
                    var sellDocLine = new SellDocLine {
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
                        TransactionUnitFactor = transUnitFactor
                    };
                    //_context.Entry(transToAttach).Entity
                    transToAttach.SellDocLines.Add(sellDocLine);

                    #endregion

                    if (!noWarehouseTrans) {
                        #region Warehouse transaction

                        var warehouseTrans = new WarehouseTransaction {
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

                try {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e) {
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    return BadRequest(new {
                        error = e.Message + " " + msg
                    });
                }
            }

            return Ok(new { });
        }
    }
}