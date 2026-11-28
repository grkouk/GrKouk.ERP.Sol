using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.PaymentMethods;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Helpers.SfGrid;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    /// <summary>
    /// Payment methods index built on Syncfusion controls: company and status filters, the
    /// Greek/English search, a grid with server-side paging and sorting, and bulk delete.
    /// Paging, sorting and lookups come from the shared <see cref="SfGrid"/> helpers.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexSfModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexSfModel(ApiDbContext context)
        {
            _context = context;
        }

        public List<SfFilterItem> CompanyFilterItems { get; set; }
        public List<SfFilterItem> ActiveFilterItems { get; } = SfIndexLookups.GetActiveItems();

        private static readonly SfSortMap<PaymentMethodListDto> Sorts = new SfSortMap<PaymentMethodListDto>(p => p.Id)
            .Add("code", p => p.Code, isDefault: true)
            .Add("name", p => p.Name)
            .Add("active", p => p.Active)
            .Add("daysOverdue", p => p.DaysOverdue)
            .Add("autoPayoffWayText", p => p.AutoPayoffWay)
            .Add("cfAccountName", p => p.CfAccountName)
            .Add("payoffSeriesName", p => p.PayoffSeriesName)
            .Add("companyCode", p => p.CompanyCode);

        public async Task OnGetAsync()
        {
            CompanyFilterItems = await SfIndexLookups.GetCompanyItemsAsync(_context);
        }

        /// <summary>
        /// Grid data source (Syncfusion UrlAdaptor). Filters arrive in <c>dm.Params</c>
        /// (companyFilter, activeFilter, searchFilter); sorting and paging are the grid's own.
        /// </summary>
        public async Task<IActionResult> OnPostDataSourceAsync([FromBody] SfGridRequest dm)
        {
            var query = PaymentMethodMappings.ToListQuery(_context).AsNoTracking();

            query = FilterEval.ApplyCompanyFilter(query, dm.GetString("companyFilter"), p => p.CompanyId);

            switch (dm.GetString("activeFilter"))
            {
                case "1":
                    query = query.Where(p => p.Active);
                    break;
                case "0":
                    query = query.Where(p => !p.Active);
                    break;
            }

            query = TextSearch.Apply(query, dm.GetString("searchFilter"),
                p => p.Code, p => p.Name, p => p.CfAccountName, p => p.PayoffSeriesName, p => p.CompanyCode);

            var (rows, count) = await SfGrid.LoadPageAsync(query, dm, Sorts);
            return SfGrid.Result(rows.Select(ToGridRow), count, dm);
        }

        /// <summary>
        /// Deletes the selected payment methods in one transaction (all or nothing). Methods used
        /// by documents are refused: the buy and sell documents would be deleted with them (cascade).
        /// </summary>
        public async Task<IActionResult> OnPostDeleteSelectedAsync([FromBody] SfDeleteSelectedRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(new { error = "Nothing selected" });
            }

            var used = await PaymentMethodUsage.GetUsedCodesAsync(_context, request.Ids);
            if (used.Count > 0)
            {
                return BadRequest(new
                {
                    error = $"Used by documents, cannot be deleted: {string.Join(", ", used)}"
                });
            }

            var toRemove = await _context.PaymentMethods
                .Where(p => request.Ids.Contains(p.Id))
                .ToListAsync();
            _context.PaymentMethods.RemoveRange(toRemove);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { error = "Some of the selected payment methods are in use and cannot be deleted." });
            }

            return new JsonResult(new { deleted = toRemove.Count });
        }

        private static object ToGridRow(PaymentMethodListDto p) => new
        {
            p.Id,
            p.Code,
            p.Name,
            p.Active,
            p.DaysOverdue,
            AutoPayoffWayText = p.AutoPayoffWay.GetDescription(),
            p.CfAccountName,
            p.PayoffSeriesName,
            p.CompanyCode
        };
    }
}
