using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NToastNotify.Helpers;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialDataController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public FinancialDataController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private static decimal ConvertAmount(int companyCurrencyId, int displayCurrencyId, IList<ExchangeRate> rates,
            decimal amount)
        {
            decimal retAmount = amount;
            if (displayCurrencyId == companyCurrencyId)
            {
                return retAmount;
            }

            if (companyCurrencyId != 1)
            {
                var r = rates.Where(p => p.CurrencyId == companyCurrencyId)
                    .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                if (r != null)
                {
                    retAmount = amount / r.Rate;
                }
            }

            if (displayCurrencyId != 1)
            {
                var r = rates.Where(p => p.CurrencyId == displayCurrencyId)
                    .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                if (r != null)
                {
                    retAmount *= r.Rate;
                }
            }

            return retAmount;
        }

        [HttpGet("GetMainDashboardInfo")]
        public async Task<IActionResult> GetMainDashboardInfo([FromQuery] IndexDataTableRequest request)
        {
           
            var codeToComputeDefinition = await
                _context.AppSettings.FirstOrDefaultAsync(p => p.Code == request.CodeToCompute);
            if (codeToComputeDefinition == null)
            {
                return BadRequest(new {Message = "Code to compute not exist"});
            }

            if (string.IsNullOrEmpty(codeToComputeDefinition.Value))
            {
                return BadRequest(new {Message = "No definition found for code to compute"});
            }

            decimal r = 0;
            var currencyRates = await _context.ExchangeRates.OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();
            var def = codeToComputeDefinition.Value;
            var defObj = JsonConvert.DeserializeObject<CodeToComputeDefinition>(def);
            if (defObj.MatNatures==null &&
                defObj.TransTypes==null &&
                defObj.DocTypesSelected==null)
            {
                    
                return Ok(new MainDashboardInfoResponse
                {
                    RequestedCodeToCompute = request.CodeToCompute,
                    RequestedCodeSum = 0
                });
            }
            if (defObj.SrcType == MainInfoSourceTypeEnum.SourceTypeBuys)
            {
                
                IQueryable<BuyDocument> fullListIq = _context.BuyDocuments
                    .Include(p => p.Transactor);
                if (!string.IsNullOrEmpty(request.CompanyFilter))
                {
                    if (int.TryParse(request.CompanyFilter, out var companyId))
                    {
                        if (companyId > 0)
                        {
                            fullListIq = fullListIq.Where(p => p.CompanyId == companyId);
                        }
                    }
                }

                DateTime beforePeriodDate = DateTime.Today;
                if (!string.IsNullOrEmpty(request.DateRange))
                {
                    var datePeriodFilter = request.DateRange;
                    DateFilterDates dfDates = DateFilter.GetDateFilterDates(datePeriodFilter);
                    DateTime fromDate = dfDates.FromDate;
                    beforePeriodDate = fromDate.AddDays(-1);
                    DateTime toDate = dfDates.ToDate;
                    fullListIq = fullListIq.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
                    //transactionsList = transactionsList.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
                    //transListBeforePeriod = transListBeforePeriod.Where(p => p.TransDate < fromDate);
                }

                if (defObj.TransTypes != null)
                {
                    if (defObj.TransTypes.Length > 0)
                    {
                        fullListIq = fullListIq.Where(p => defObj.TransTypes.Contains(p.Transactor.TransactorTypeId));
                    }
                }


                if (defObj.DocTypesSelected != null)
                {
                    if (defObj.DocTypesSelected.Length > 0)
                    {
                        fullListIq = fullListIq.Where(p => defObj.DocTypesSelected.Contains(p.BuyDocTypeId));
                    }
                }


                var t = fullListIq.ProjectTo<BuyDocListDto>(_mapper.ConfigurationProvider);
                var t1 = await t.Select(p => new BuyDocListDto
                {
                    AmountFpa = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountFpa),
                    AmountNet = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountNet),
                    AmountDiscount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountDiscount),
                    CompanyCurrencyId = p.CompanyCurrencyId
                }).ToListAsync();
                //var grandSumOfAmount = t1.Sum(p => p.TotalAmount);
                r = t1.Sum(p => p.TotalNetAmount);
            }

            if (defObj.SrcType == MainInfoSourceTypeEnum.SourceTypeSales)
            {
                
                IQueryable<SellDocument> fullListIq = _context.SellDocuments
                    .Include(p => p.Transactor);

                if (!string.IsNullOrEmpty(request.CompanyFilter))
                {
                    if (int.TryParse(request.CompanyFilter, out var companyId))
                    {
                        if (companyId > 0)
                        {
                            fullListIq = fullListIq.Where(p => p.CompanyId == companyId);
                        }
                    }
                }

                DateTime beforePeriodDate = DateTime.Today;
                if (!string.IsNullOrEmpty(request.DateRange))
                {
                    var datePeriodFilter = request.DateRange;
                    DateFilterDates dfDates = DateFilter.GetDateFilterDates(datePeriodFilter);
                    DateTime fromDate = dfDates.FromDate;
                    beforePeriodDate = fromDate.AddDays(-1);
                    DateTime toDate = dfDates.ToDate;
                    fullListIq = fullListIq.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
                    //transactionsList = transactionsList.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
                    //transListBeforePeriod = transListBeforePeriod.Where(p => p.TransDate < fromDate);
                }

                if (defObj.TransTypes != null)
                {
                    if (defObj.TransTypes.Length > 0)
                    {
                        fullListIq = fullListIq.Where(p => defObj.TransTypes.Contains(p.Transactor.TransactorTypeId));
                    }
                }

                if (defObj.DocTypesSelected != null)
                {
                    if (defObj.DocTypesSelected.Length > 0)
                    {
                        fullListIq = fullListIq.Where(p => defObj.DocTypesSelected.Contains(p.SellDocTypeId));
                    }
                }


                var t = fullListIq.ProjectTo<SellDocListDto>(_mapper.ConfigurationProvider);
                var t1 = await t.Select(p => new SellDocListDto
                {
                    AmountFpa = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountFpa),
                    AmountNet = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountNet),
                    AmountDiscount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                        p.AmountDiscount),
                    CompanyCurrencyId = p.CompanyCurrencyId
                }).ToListAsync();
                //var grandSumOfAmount = t1.Sum(p => p.TotalAmount);
                r = t1.Sum(p => p.TotalNetAmount);
            }

            var response = new MainDashboardInfoResponse
            {
                RequestedCodeToCompute = request.CodeToCompute,
                RequestedCodeSum = r
            };
            return Ok(response);
        }

        [HttpGet("GetTransactorFinancialSummaryData")]
        public async Task<IActionResult> GetTransactorFinancialSummaryData([FromQuery] IndexDataTableRequest request)
        {
            if (request.TransactorId <= 0)
            {
                return BadRequest();
            }

            IQueryable<TransactorTransaction> fullListIq =
                _context.TransactorTransactions.Where(p => p.TransactorId == request.TransactorId);
            if (!string.IsNullOrEmpty(request.CompanyFilter))
            {
                if (int.TryParse(request.CompanyFilter, out var companyId))
                {
                    if (companyId > 0)
                    {
                        fullListIq = fullListIq.Where(p => p.CompanyId == companyId);
                    }
                }
            }

            var currencyRates = await _context.ExchangeRates.OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();
            var t = fullListIq.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);
            var t1 = await t.Select(p => new TransactorTransListDto
            {
                TransTransactorDocSeriesId = p.TransTransactorDocSeriesId,
                TransTransactorDocSeriesName = p.TransTransactorDocSeriesName,
                TransTransactorDocSeriesCode = p.TransTransactorDocSeriesCode,
                TransTransactorDocTypeId = p.TransTransactorDocTypeId,
                FinancialAction = p.FinancialAction,
                AmountFpa = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates, p.AmountFpa),
                AmountNet = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates, p.AmountNet),
                AmountDiscount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.AmountDiscount),
                TransFpaAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransFpaAmount),
                TransNetAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransNetAmount),
                TransDiscountAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransDiscountAmount),
                CompanyCode = p.CompanyCode,
                CompanyCurrencyId = p.CompanyCurrencyId
            }).ToListAsync();
            var grandSumOfAmount = t1.Sum(p => p.TotalAmount);
            var grandSumOfDebit = t1.Sum(p => p.DebitAmount);
            var grandSumOfCredit = t1.Sum(p => p.CreditAmount);
            var transactor = await _context.Transactors.Include(p => p.TransactorType)
                .FirstOrDefaultAsync(p => p.Id == request.TransactorId);
            if (transactor == null)
            {
                return BadRequest();
            }

            await _context.Entry(transactor).Reference(p => p.TransactorType).LoadAsync();
            var transactorType = transactor.TransactorType;
            if (transactorType == null)
            {
                return BadRequest();
            }

            decimal difference = 0;
            switch (transactorType.Code)
            {
                case "SYS.CUSTOMER":
                    difference = grandSumOfDebit - grandSumOfCredit;
                    break;
                case "SYS.SUPPLIER":
                    difference = grandSumOfCredit - grandSumOfDebit;
                    break;
                case "SYS.DEPARTMENT":
                    difference = grandSumOfDebit - grandSumOfCredit;
                    break;
                case "SYS.DTRANSACTOR":
                    difference = grandSumOfCredit - grandSumOfDebit;
                    break;
                default:
                    break;
            }

            var response = new TransactorFinancialDataResponse()
            {
                SumOfDebit = grandSumOfDebit,
                SumOfCredit = grandSumOfCredit,
                SumOfDifference = difference
            };
            return Ok(response);
        }

        /// <summary>
        /// Only for testing purposes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetTransactions2")]
        public async Task<IActionResult> GetTransactions2([FromBody] DataManagerRequest request)
        {
            Debug.Print(request.ToJson());
            return new JsonResult(new {result = "", count = 0});
        }

        [HttpGet("GetTransactorTransactions")]
        public async Task<IActionResult> GetTransactorTransactions([FromQuery] IndexDataTableRequest request)
        {
            if (request.TransactorId <= 0)
            {
                return BadRequest(new
                {
                    Error = "No valid transactor id specified"
                });
            }

            var transactor = await _context.Transactors.FirstOrDefaultAsync(x => x.Id == request.TransactorId);
            if (transactor == null)
            {
                return NotFound(new
                {
                    Error = "Transactor not found"
                });
            }

            var transactorType = await _context.TransactorTypes.Where(c => c.Id == transactor.TransactorTypeId)
                .FirstOrDefaultAsync();

            IQueryable<TransactorTransaction> transactionsList = _context.TransactorTransactions
                .Where(p => p.TransactorId == request.TransactorId);
            IQueryable<TransactorTransaction> transListBeforePeriod = _context.TransactorTransactions
                .Where(p => p.TransactorId == request.TransactorId);
            IQueryable<TransactorTransaction> transListAll = _context.TransactorTransactions
                .Where(p => p.TransactorId == request.TransactorId);
            //default order 
            transactionsList = transactionsList.OrderByDescending(p => p.TransDate);
            if (!string.IsNullOrEmpty(request.SortData))
            {
                switch (request.SortData.ToLower())
                {
                    case "datesort:asc":
                        transactionsList = transactionsList.OrderBy(p => p.TransDate);
                        break;
                    case "datesort:desc":
                        transactionsList = transactionsList.OrderByDescending(p => p.TransDate);
                        break;
                    case "transactornamesort:asc":
                        transactionsList = transactionsList.OrderBy(p => p.Transactor.Name);
                        break;
                    case "transactornamesort:desc":
                        transactionsList = transactionsList.OrderByDescending(p => p.Transactor.Name);
                        break;
                    case "seriescodesort:asc":
                        transactionsList = transactionsList.OrderBy(p => p.TransTransactorDocSeries.Code);
                        break;
                    case "seriescodesort:desc":
                        transactionsList = transactionsList.OrderByDescending(p => p.TransTransactorDocSeries.Code);
                        break;
                    case "companycodesort:asc":
                        transactionsList = transactionsList.OrderBy(p => p.Company.Code);
                        break;
                    case "companycodesort:desc":
                        transactionsList = transactionsList.OrderByDescending(p => p.Company.Code);
                        break;
                }
            }

            DateTime beforePeriodDate = DateTime.Today;
            if (!string.IsNullOrEmpty(request.DateRange))
            {
                var datePeriodFilter = request.DateRange;
                DateFilterDates dfDates = DateFilter.GetDateFilterDates(datePeriodFilter);
                DateTime fromDate = dfDates.FromDate;
                beforePeriodDate = fromDate.AddDays(-1);
                DateTime toDate = dfDates.ToDate;

                transactionsList = transactionsList.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
                transListBeforePeriod = transListBeforePeriod.Where(p => p.TransDate < fromDate);
            }

            if (!string.IsNullOrEmpty(request.CompanyFilter))
            {
                if (int.TryParse(request.CompanyFilter, out var companyId))
                {
                    if (companyId > 0)
                    {
                        transactionsList = transactionsList.Where(p => p.CompanyId == companyId);
                        transListBeforePeriod = transListBeforePeriod.Where(p => p.CompanyId == companyId);
                        transListAll = transListAll.Where(p => p.CompanyId == companyId);
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.SearchFilter))
            {
                transactionsList = transactionsList.Where(p =>
                    p.TransTransactorDocSeries.Name.Contains(request.SearchFilter)
                    || p.TransTransactorDocSeries.Code.Contains(request.SearchFilter)
                    || p.TransRefCode.Contains(request.SearchFilter)
                );
                transListBeforePeriod = transListBeforePeriod.Where(p =>
                    p.TransTransactorDocSeries.Name.Contains(request.SearchFilter)
                    || p.TransTransactorDocSeries.Code.Contains(request.SearchFilter)
                    || p.TransRefCode.Contains(request.SearchFilter)
                );
                transListAll = transListAll.Where(p => p.TransTransactorDocSeries.Name.Contains(request.SearchFilter)
                                                       || p.TransTransactorDocSeries.Code.Contains(request.SearchFilter)
                                                       || p.TransRefCode.Contains(request.SearchFilter)
                );
            }

            var dbTrans = transactionsList.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);

            var currencyRates = await _context.ExchangeRates.OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();
            var t = transListAll.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);
            var t1 = await t.Select(p => new TransactorTransListDto
            {
                Id = p.Id,
                TransDate = p.TransDate,
                TransTransactorDocSeriesId = p.TransTransactorDocSeriesId,
                TransTransactorDocSeriesName = p.TransTransactorDocSeriesName,
                TransTransactorDocSeriesCode = p.TransTransactorDocSeriesCode,
                TransTransactorDocTypeId = p.TransTransactorDocTypeId,
                TransRefCode = p.TransRefCode,
                TransactorId = p.TransactorId,
                TransactorName = p.TransactorName,
                SectionId = p.SectionId,
                SectionCode = p.SectionCode,
                CreatorId = p.CreatorId,
                FiscalPeriodId = p.FiscalPeriodId,
                FinancialAction = p.FinancialAction,
                FpaRate = p.FpaRate,
                DiscountRate = p.DiscountRate,
                AmountFpa = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates, p.AmountFpa),
                AmountNet = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates, p.AmountNet),
                AmountDiscount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.AmountDiscount),
                TransFpaAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransFpaAmount),
                TransNetAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransNetAmount),
                TransDiscountAmount = ConvertAmount(p.CompanyCurrencyId, request.DisplayCurrencyId, currencyRates,
                    p.TransDiscountAmount),
                CompanyCode = p.CompanyCode,
                CompanyCurrencyId = p.CompanyCurrencyId
            }).ToListAsync();
            var grandSumOfAmount = t1.Sum(p => p.TotalAmount);
            var grandSumOfDebit = t1.Sum(p => p.DebitAmount);
            var grandSumOfCredit = t1.Sum(p => p.CreditAmount);
            var dbTransactions = await dbTrans.ToListAsync();
            foreach (var listItem in dbTransactions)
            {
                if (listItem.CompanyCurrencyId != 1)
                {
                    var r = currencyRates.Where(p => p.CurrencyId == listItem.CompanyCurrencyId)
                        .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                    if (r != null)
                    {
                        listItem.AmountFpa = listItem.AmountFpa / r.Rate;
                        listItem.AmountNet = listItem.AmountNet / r.Rate;
                        listItem.AmountDiscount = listItem.AmountDiscount / r.Rate;
                        listItem.TransFpaAmount = listItem.TransFpaAmount / r.Rate;
                        listItem.TransNetAmount = listItem.TransNetAmount / r.Rate;
                        listItem.TransDiscountAmount = listItem.TransDiscountAmount / r.Rate;
                    }
                }

                if (request.DisplayCurrencyId != 1)
                {
                    var r = currencyRates.Where(p => p.CurrencyId == request.DisplayCurrencyId)
                        .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                    if (r != null)
                    {
                        listItem.AmountFpa = listItem.AmountFpa * r.Rate;
                        listItem.AmountNet = listItem.AmountNet * r.Rate;
                        listItem.AmountDiscount = listItem.AmountDiscount * r.Rate;
                        listItem.TransFpaAmount = listItem.TransFpaAmount * r.Rate;
                        listItem.TransNetAmount = listItem.TransNetAmount * r.Rate;
                        listItem.TransDiscountAmount = listItem.TransDiscountAmount * r.Rate;
                    }
                }
            }

            //-----------------------------------------------
            var dbTransBeforePeriod =
                transListBeforePeriod.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);
            var transBeforePeriodList = await dbTransBeforePeriod.ToListAsync();
            foreach (var item in transBeforePeriodList)
            {
                if (item.CompanyCurrencyId != 1)
                {
                    var r = currencyRates.Where(p => p.CurrencyId == item.CompanyCurrencyId)
                        .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                    if (r != null)
                    {
                        item.AmountFpa = item.AmountFpa / r.Rate;
                        item.AmountNet = item.AmountNet / r.Rate;
                        item.AmountDiscount = item.AmountDiscount / r.Rate;
                        item.TransFpaAmount = item.TransFpaAmount / r.Rate;
                        item.TransNetAmount = item.TransNetAmount / r.Rate;
                        item.TransDiscountAmount = item.TransDiscountAmount / r.Rate;
                    }
                }

                if (request.DisplayCurrencyId != 1)
                {
                    var r = currencyRates.Where(p => p.CurrencyId == request.DisplayCurrencyId)
                        .OrderByDescending(p => p.ClosingDate).FirstOrDefault();
                    if (r != null)
                    {
                        item.AmountFpa = item.AmountFpa * r.Rate;
                        item.AmountNet = item.AmountNet * r.Rate;
                        item.AmountDiscount = item.AmountDiscount * r.Rate;
                        item.TransFpaAmount = item.TransFpaAmount * r.Rate;
                        item.TransNetAmount = item.TransNetAmount * r.Rate;
                        item.TransDiscountAmount = item.TransDiscountAmount * r.Rate;
                    }
                }
            }

            //Create before period line
            var bl1 = new
            {
                Debit = transBeforePeriodList.Sum(x => x.DebitAmount),
                Credit = transBeforePeriodList.Sum(x => x.CreditAmount),
            };

            var beforePeriod = new KartelaLine();

            beforePeriod.Credit = bl1.Credit;
            beforePeriod.Debit = bl1.Debit;
            switch (transactorType.Code)
            {
                case "SYS.DTRANSACTOR":

                    break;
                case "SYS.CUSTOMER":
                    beforePeriod.RunningTotal = bl1.Debit - bl1.Credit;
                    break;
                case "SYS.SUPPLIER":
                    beforePeriod.RunningTotal = bl1.Credit - bl1.Debit;
                    break;
                default:
                    beforePeriod.RunningTotal = bl1.Credit - bl1.Debit;
                    break;
            }

            beforePeriod.TransDate = beforePeriodDate;
            beforePeriod.DocSeriesCode = "Εκ.Μεταφ.";
            beforePeriod.CreatorId = -1;
            beforePeriod.TransactorName = "";

            var listWithTotal = new List<KartelaLine>();
            listWithTotal.Add(beforePeriod);

            //----------------------------------------------------


            decimal runningTotal = beforePeriod.RunningTotal;
            foreach (var dbTransaction in dbTransactions)
            {
                switch (transactorType.Code)
                {
                    case "SYS.DTRANSACTOR":

                        break;
                    case "SYS.CUSTOMER":
                        runningTotal = dbTransaction.DebitAmount - dbTransaction.CreditAmount + runningTotal;
                        break;
                    case "SYS.SUPPLIER":
                        runningTotal = dbTransaction.CreditAmount - dbTransaction.DebitAmount + runningTotal;
                        break;
                    default:
                        runningTotal = dbTransaction.CreditAmount - dbTransaction.DebitAmount + runningTotal;
                        break;
                }


                listWithTotal.Add(new KartelaLine
                {
                    TransDate = dbTransaction.TransDate,
                    DocSeriesCode = dbTransaction.TransTransactorDocSeriesCode,
                    RefCode = dbTransaction.TransRefCode,
                    CompanyCode = dbTransaction.CompanyCode,
                    SectionCode = dbTransaction.SectionCode,
                    CreatorId = dbTransaction.SectionCode == "SCNTRANSACTORTRANS"
                        ? dbTransaction.Id
                        : dbTransaction.CreatorId,
                    RunningTotal = runningTotal,
                    TransactorName = dbTransaction.TransactorName,
                    Debit = dbTransaction.DebitAmount,
                    Credit = dbTransaction.CreditAmount
                });
            }

            var outList = listWithTotal.AsQueryable();
            var pageIndex = request.PageIndex;

            var pageSize = request.PageSize;
            decimal sumCredit = 0;
            decimal sumDebit = 0;
            decimal sumDifference = 0;

            IQueryable<KartelaLine> fullListIq = from s in outList select s;

            var listItems = fullListIq.ToList();

            foreach (var item in listItems)
            {
                sumCredit += item.Credit;
                sumDebit += item.Debit;
            }

            switch (transactorType.Code)
            {
                case "SYS.DTRANSACTOR":

                    break;
                case "SYS.CUSTOMER":
                    sumDifference = sumDebit - sumCredit;
                    break;
                case "SYS.SUPPLIER":
                    sumDifference = sumCredit - sumDebit;
                    break;
                default:
                    sumDifference = sumCredit - sumDebit;
                    break;
            }

            var response = new JsonResult(listItems);

            return Ok(response);
        }
    }
    public class LineChartData
    {
        public DateTime XValue;
        public double YValue;
        public double YValue1;
    }
}