using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    /// <summary>
    /// Sell documents index built on Syncfusion controls: combos for the filters, a grid with
    /// server-side paging and sorting, amounts converted to the selected display currency,
    /// page and grand totals, bulk delete, payoff and receipt assignment.
    /// </summary>
    [Authorize(Roles = "Admin,Operator")]
    public class IndexSfModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IDocumentTransactionService _docTransSrv;

        public IndexSfModel(ApiDbContext context, IDocumentTransactionService docTransSrv)
        {
            _context = context;
            _docTransSrv = docTransSrv;
        }

        public List<FilterItem> CompanyFilterItems { get; set; }
        public List<FilterItem> DatePeriodItems { get; set; }
        public List<CurrencyItem> CurrencyItems { get; set; }
        public List<FilterItem> Diaries { get; set; }

        public async Task OnGetAsync()
        {
            CompanyFilterItems = (await FiltersHelper.GetCompaniesFilterListAsync(_context))
                .Select(p => new FilterItem { Value = p.Value, Text = p.Text })
                .ToList();
            // The shared list repeats some "last year" months; a combo needs unique values.
            DatePeriodItems = DateFilter.GetDateFiltersSelectList()
                .DistinctBy(p => p.Value)
                .Select(p => new FilterItem { Value = p.Value, Text = p.Text })
                .ToList();
            CurrencyItems = await _context.Currencies.AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new CurrencyItem
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    DisplayLocale = p.DisplayLocale
                })
                .ToListAsync();
            Diaries = await _context.DiaryDefs.AsNoTracking()
                .Where(p => p.DiaryType == DiaryTypeEnum.DiaryTypeEnumSales)
                .OrderBy(p => p.Name)
                .Select(p => new FilterItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
        }

        /// <summary>
        /// Grid data source (Syncfusion UrlAdaptor). Filters arrive in <c>dm.Params</c>;
        /// sorting and paging are the grid's own. Amounts come back in the display currency.
        /// </summary>
        public async Task<IActionResult> OnPostDataSourceAsync([FromBody] GridRequest dm)
        {
            var filters = SellDocFilters.FromParams(dm.Params);
            IQueryable<SellDocList2Dto> query;
            try
            {
                query = ApplyFilters(filters);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { error = e.Message });
            }

            var count = await query.CountAsync();

            query = ApplySort(query, dm.Sorted);
            if (dm.Skip > 0)
            {
                query = query.Skip(dm.Skip);
            }
            if (dm.Take > 0)
            {
                query = query.Take(dm.Take);
            }

            var rows = await query.ToListAsync();
            var rates = await GetRecentRatesAsync();
            var result = rows.Select(p => ToGridRow(p, filters.DisplayCurrencyId, rates)).ToList();

            return dm.RequiresCounts
                ? new JsonResult(new { result, count })
                : new JsonResult(result);
        }

        /// <summary>Grand totals of all the documents that match the filters, in the display currency.</summary>
        public async Task<IActionResult> OnPostTotalsAsync([FromBody] SellDocFilters filters)
        {
            filters ??= new SellDocFilters();
            IQueryable<SellDocList2Dto> query;
            try
            {
                query = ApplyFilters(filters);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { error = e.Message });
            }

            // Converting per company currency, so sum in memory over the few amount columns only.
            var amounts = await query
                .Select(p => new
                {
                    p.CompanyCurrencyId,
                    p.AmountNet,
                    p.AmountFpa,
                    p.AmountDiscount,
                    p.PayedOfAmount
                })
                .ToListAsync();
            var rates = await GetRecentRatesAsync();
            decimal Convert(int currencyId, decimal amount) =>
                CurrencyConverter.ConvertAmount(currencyId, filters.DisplayCurrencyId, rates, amount);

            return new JsonResult(new
            {
                count = amounts.Count,
                netAmount = amounts.Sum(p => Convert(p.CompanyCurrencyId, p.AmountNet - p.AmountDiscount)),
                amount = amounts.Sum(p =>
                    Convert(p.CompanyCurrencyId, p.AmountNet + p.AmountFpa - p.AmountDiscount)),
                payedAmount = amounts.Sum(p => Convert(p.CompanyCurrencyId, p.PayedOfAmount))
            });
        }

        /// <summary>
        /// Deletes the selected documents with their lines and related transactions,
        /// in one transaction (all or nothing).
        /// </summary>
        public async Task<IActionResult> OnPostDeleteSelectedAsync([FromBody] DeleteSelectedRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(new { error = "Nothing selected" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var id in request.Ids)
            {
                var srvResult = await _docTransSrv.DeleteSaleDocument(id);
                if (!srvResult.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = srvResult.ErrorMessage });
                }
            }
            await transaction.CommitAsync();

            return new JsonResult(new { deleted = request.Ids.Count });
        }

        private IQueryable<SellDocList2Dto> ApplyFilters(SellDocFilters filters)
        {
            var query = _context.SellDocuments.AsNoTracking()
                .Select(SellDocumentMappings.ToList2Dto);

            var (fromDate, toDate) = FilterEval.GetDateRange(filters.DateRange, filters.FromDate, filters.ToDate);
            if (fromDate.HasValue && toDate.HasValue)
            {
                var from = fromDate.Value;
                var to = toDate.Value;
                query = query.Where(p => p.TransDate >= from && p.TransDate <= to);
            }

            query = FilterEval.ApplyCompanyFilter(query, filters.CompanyFilter, p => p.CompanyId);
            query = FilterEval.ApplySearchFilter(query, filters.SearchFilter,
                p => p.TransactorName, p => p.TransRefCode);
            return query;
        }

        private Task<List<ExchangeRate>> GetRecentRatesAsync() =>
            _context.ExchangeRates.AsNoTracking()
                .OrderByDescending(p => p.ClosingDate)
                .Take(10)
                .ToListAsync();

        private static GridRow ToGridRow(SellDocList2Dto src, int displayCurrencyId, IList<ExchangeRate> rates)
        {
            decimal Convert(decimal amount) =>
                CurrencyConverter.ConvertAmount(src.CompanyCurrencyId, displayCurrencyId, rates, amount);

            return new GridRow
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransDateText = FormatGridDate(src.TransDate),
                TransRefCode = src.TransRefCode,
                TransactorId = src.TransactorId,
                TransactorName = src.TransactorName,
                SellDocSeriesCode = src.SellDocSeriesCode,
                SellDocSeriesName = src.SellDocSeriesName,
                CompanyCode = src.CompanyCode,
                TotalNetAmount = Convert(src.TotalNetAmount),
                TotalAmount = Convert(src.TotalAmount),
                PayedOfAmount = Convert(src.PayedOfAmount),
                // Same rule as the old page: payoff/assign receipts only while something is unpaid.
                HasBalance = src.TotalAmount - src.PayedOfAmount != 0
            };
        }

        private static readonly string[] DayAbbreviations = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

        /// <summary>Grid date text: two-letter day name plus date, e.g. "Mo 27/12/2026".</summary>
        private static string FormatGridDate(DateTime date) =>
            $"{DayAbbreviations[(int)date.DayOfWeek]} {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";

        /// <summary>Single-column sort; Id keeps paging stable. Amount columns are not sortable
        /// (they are converted per company currency after loading).</summary>
        private static IQueryable<SellDocList2Dto> ApplySort(IQueryable<SellDocList2Dto> query, List<Sort> sorted)
        {
            var sort = sorted?.FirstOrDefault();
            var desc = string.Equals(sort?.Direction, "descending", StringComparison.OrdinalIgnoreCase);
            IOrderedQueryable<SellDocList2Dto> ordered = (sort?.Name ?? "").ToLowerInvariant() switch
            {
                "transactorname" => desc
                    ? query.OrderByDescending(p => p.TransactorName)
                    : query.OrderBy(p => p.TransactorName),
                "selldocseriescode" => desc
                    ? query.OrderByDescending(p => p.SellDocSeriesCode)
                    : query.OrderBy(p => p.SellDocSeriesCode),
                "transrefcode" => desc
                    ? query.OrderByDescending(p => p.TransRefCode)
                    : query.OrderBy(p => p.TransRefCode),
                "companycode" => desc
                    ? query.OrderByDescending(p => p.CompanyCode)
                    : query.OrderBy(p => p.CompanyCode),
                _ => desc ? query.OrderByDescending(p => p.TransDate) : query.OrderBy(p => p.TransDate)
            };
            return ordered.ThenBy(p => p.Id);
        }

        public class FilterItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class CurrencyItem
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string DisplayLocale { get; set; }
        }

        /// <summary>The page filters, posted as the grid's params or as the totals request body.</summary>
        public class SellDocFilters
        {
            public string DateRange { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string CompanyFilter { get; set; }
            public string SearchFilter { get; set; }
            public int DisplayCurrencyId { get; set; } = 1;

            public static SellDocFilters FromParams(Dictionary<string, object> values)
            {
                string Get(string key) =>
                    values != null && values.TryGetValue(key, out var value) && value != null
                        ? System.Convert.ToString(value, CultureInfo.InvariantCulture)
                        : null;

                DateTime? GetDate(string key) =>
                    DateTime.TryParse(Get(key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                        ? d
                        : null;

                return new SellDocFilters
                {
                    DateRange = Get("dateRange"),
                    FromDate = GetDate("fromDate"),
                    ToDate = GetDate("toDate"),
                    CompanyFilter = Get("companyFilter"),
                    SearchFilter = Get("searchFilter"),
                    DisplayCurrencyId = int.TryParse(Get("displayCurrencyId"), out var c) && c > 0 ? c : 1
                };
            }
        }

        public class GridRow
        {
            public int Id { get; set; }
            public DateTime TransDate { get; set; }
            public string TransDateText { get; set; }
            public string TransRefCode { get; set; }
            public int TransactorId { get; set; }
            public string TransactorName { get; set; }
            public string SellDocSeriesCode { get; set; }
            public string SellDocSeriesName { get; set; }
            public string CompanyCode { get; set; }
            public decimal TotalNetAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal PayedOfAmount { get; set; }
            public bool HasBalance { get; set; }
        }

        /// <summary>Grid request plus the values added with Query.addParams (sent as "params").</summary>
        public class GridRequest : DataManagerRequest
        {
            public Dictionary<string, object> Params { get; set; }
        }

        public class DeleteSelectedRequest
        {
            public List<int> Ids { get; set; }
        }
    }
}
