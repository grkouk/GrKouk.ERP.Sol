using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.DocTypeDefs;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocTypeDefs
{
    /// <summary>
    /// Index of sell document type definitions built on Syncfusion controls: combos for the filters,
    /// a grid with server-side paging, sorting and search, and row selection with bulk delete.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexSfModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexSfModel(ApiDbContext context)
        {
            _context = context;
        }

        public List<FilterItem> CompanyFilterItems { get; set; }
        public List<FilterItem> ActiveFilterItems { get; } = new()
        {
            new FilterItem { Value = "", Text = "{All}" },
            new FilterItem { Value = "1", Text = "Active" },
            new FilterItem { Value = "0", Text = "Inactive" }
        };

        public async Task OnGetAsync()
        {
            CompanyFilterItems = (await FiltersHelper.GetCompaniesFilterListAsync(_context))
                .Select(p => new FilterItem { Value = p.Value, Text = p.Text })
                .ToList();
        }

        /// <summary>
        /// Grid data source (Syncfusion UrlAdaptor). Filters arrive in <c>dm.Params</c>
        /// (companyFilter, activeFilter, searchFilter); sorting and paging are the grid's own.
        /// </summary>
        public async Task<IActionResult> OnPostDataSourceAsync([FromBody] GridRequest dm)
        {
            var query = _context.SellDocTypeDefs.AsNoTracking()
                .Select(SellDocTypeDefMappings.ToListDto);

            query = FilterEval.ApplyCompanyFilter(query, GetParam(dm, "companyFilter"), p => p.CompanyId);

            switch (GetParam(dm, "activeFilter"))
            {
                case "1":
                    query = query.Where(p => p.Active);
                    break;
                case "0":
                    query = query.Where(p => !p.Active);
                    break;
            }

            query = FilterEval.ApplySearchFilter(query, GetParam(dm, "searchFilter"),
                p => p.Code, p => p.Abbr, p => p.Name);

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

            var result = await query.ToListAsync();
            return dm.RequiresCounts
                ? new JsonResult(new { result, count })
                : new JsonResult(result);
        }

        /// <summary>Deletes the selected definitions in one transaction (all or nothing).</summary>
        public async Task<IActionResult> OnPostDeleteSelectedAsync([FromBody] DeleteSelectedRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(new { error = "Nothing selected" });
            }

            var toRemove = await _context.SellDocTypeDefs
                .Where(p => request.Ids.Contains(p.Id))
                .ToListAsync();
            _context.SellDocTypeDefs.RemoveRange(toRemove);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    error = "Some of the selected types are used by documents or series and cannot be deleted."
                });
            }

            return new JsonResult(new { deleted = toRemove.Count });
        }

        private static string GetParam(GridRequest dm, string key)
        {
            if (dm.Params == null || !dm.Params.TryGetValue(key, out var value) || value == null)
            {
                return null;
            }
            return Convert.ToString(value);
        }

        /// <summary>Single-column sort, as on the other index pages; Id keeps paging stable.</summary>
        private static IQueryable<SellDocTypeDefListDto> ApplySort(IQueryable<SellDocTypeDefListDto> query,
            List<Sort> sorted)
        {
            var sort = sorted?.FirstOrDefault();
            var desc = string.Equals(sort?.Direction, "descending", StringComparison.OrdinalIgnoreCase);
            IOrderedQueryable<SellDocTypeDefListDto> ordered = (sort?.Name ?? "").ToLowerInvariant() switch
            {
                "abbr" => desc ? query.OrderByDescending(p => p.Abbr) : query.OrderBy(p => p.Abbr),
                "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "active" => desc ? query.OrderByDescending(p => p.Active) : query.OrderBy(p => p.Active),
                "transtransactordefname" => desc
                    ? query.OrderByDescending(p => p.TransTransactorDefName)
                    : query.OrderBy(p => p.TransTransactorDefName),
                "transwarehousedefname" => desc
                    ? query.OrderByDescending(p => p.TransWarehouseDefName)
                    : query.OrderBy(p => p.TransWarehouseDefName),
                "companycode" => desc ? query.OrderByDescending(p => p.CompanyCode) : query.OrderBy(p => p.CompanyCode),
                _ => desc ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code)
            };
            return ordered.ThenBy(p => p.Id);
        }

        public class FilterItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
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
