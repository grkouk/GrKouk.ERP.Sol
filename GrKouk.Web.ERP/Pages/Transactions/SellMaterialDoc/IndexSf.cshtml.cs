using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Helpers.SfGrid;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    /// <summary>
    /// Sell documents index built on Syncfusion controls: combos for the filters, a grid with
    /// server-side paging and sorting, amounts converted to the selected display currency,
    /// page and grand totals, bulk delete, payoff and receipt assignment.
    /// Paging, sorting, lookups and the search come from the shared <see cref="SfGrid"/> helpers.
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

        public List<SfFilterItem> CompanyFilterItems { get; set; }
        public List<SfFilterItem> DatePeriodItems { get; set; }
        public List<SfCurrencyItem> CurrencyItems { get; set; }
        public List<SfFilterItem> Diaries { get; set; }

        /// <summary>Sortable grid columns. Amounts are not sortable: they are converted per
        /// company currency after loading.</summary>
        private static readonly SfSortMap<SellDocList2Dto> Sorts = new SfSortMap<SellDocList2Dto>(p => p.Id)
            .Add("transDate", p => p.TransDate, isDefault: true)
            .Add("transactorName", p => p.TransactorName)
            .Add("sellDocSeriesCode", p => p.SellDocSeriesCode)
            .Add("transRefCode", p => p.TransRefCode)
            .Add("companyCode", p => p.CompanyCode);

        public async Task OnGetAsync()
        {
            CompanyFilterItems = await SfIndexLookups.GetCompanyItemsAsync(_context);
            DatePeriodItems = SfIndexLookups.GetDatePeriodItems();
            CurrencyItems = await SfIndexLookups.GetCurrencyItemsAsync(_context);
            Diaries = await SfIndexLookups.GetDiaryItemsAsync(_context, DiaryTypeEnum.DiaryTypeEnumSales);
        }

        /// <summary>
        /// Grid data source (Syncfusion UrlAdaptor). Filters arrive in <c>dm.Params</c>;
        /// sorting and paging are the grid's own. Amounts come back in the display currency.
        /// </summary>
        public async Task<IActionResult> OnPostDataSourceAsync([FromBody] SfGridRequest dm)
        {
            var filters = SfDocIndexFilters.FromRequest(dm);
            IQueryable<SellDocList2Dto> query;
            try
            {
                query = await ApplyFiltersAsync(filters);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { error = e.Message });
            }

            var (rows, count) = await SfGrid.LoadPageAsync(query, dm, Sorts);
            var rates = await SfIndexLookups.GetRecentRatesAsync(_context);
            return SfGrid.Result(rows.Select(p => ToGridRow(p, filters.DisplayCurrencyId, rates)), count, dm);
        }

        /// <summary>Grand totals of all the documents that match the filters, in the display currency.</summary>
        public async Task<IActionResult> OnPostTotalsAsync([FromBody] SfDocIndexFilters filters)
        {
            filters ??= new SfDocIndexFilters();
            IQueryable<SellDocList2Dto> query;
            try
            {
                query = await ApplyFiltersAsync(filters);
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
            var rates = await SfIndexLookups.GetRecentRatesAsync(_context);
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
        public async Task<IActionResult> OnPostDeleteSelectedAsync([FromBody] SfDeleteSelectedRequest request)
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

        private async Task<IQueryable<SellDocList2Dto>> ApplyFiltersAsync(SfDocIndexFilters filters)
        {
            var documents = await SearchSpec(_context)
                .ApplyAsync(_context.SellDocuments.AsNoTracking(), filters.SearchFilter);
            var query = documents.Select(SellDocumentMappings.ToList2Dto);

            var (fromDate, toDate) = FilterEval.GetDateRange(filters.DateRange, filters.FromDate, filters.ToDate);
            if (fromDate.HasValue && toDate.HasValue)
            {
                var from = fromDate.Value;
                var to = toDate.Value;
                query = query.Where(p => p.TransDate >= from && p.TransDate <= to);
            }

            return FilterEval.ApplyCompanyFilter(query, filters.CompanyFilter, p => p.CompanyId);
        }

        /// <summary>
        /// The search box looks in every text column the grid shows. Transactor, series and
        /// company are small tables: they are searched first and the documents
        /// filtered by their ids, which is much faster than testing the joined columns per document.
        /// </summary>
        public static TextSearchSpec<SellDocument> SearchSpec(ApiDbContext context) =>
            TextSearch.For<SellDocument>()
                .Column(b => b.TransRefCode)
                .Lookup(context.Transactors, t => t.Id, b => b.TransactorId, t => t.Name)
                .Lookup(context.SellDocSeriesDefs, s => s.Id, b => b.SellDocSeriesId, s => s.Code, s => s.Name)
                .Lookup(context.Companies, c => c.Id, b => b.CompanyId, c => c.Code);

        private static GridRow ToGridRow(SellDocList2Dto src, int displayCurrencyId, IList<ExchangeRate> rates)
        {
            decimal Convert(decimal amount) =>
                CurrencyConverter.ConvertAmount(src.CompanyCurrencyId, displayCurrencyId, rates, amount);

            return new GridRow
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransDateText = SfIndexLookups.FormatGridDate(src.TransDate),
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
    }
}
