using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.ErpFinancialAggregates
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<ErpFinancialAggregateDef> Defs { get; set; } = new List<ErpFinancialAggregateDef>();

        public class CoverageGapRow
        {
            public WarehouseItemNatureEnum Nature { get; set; }
            public int FpaDefId { get; set; }
            public string FpaDefName { get; set; }
            public int ItemCount { get; set; }
        }
        public IList<CoverageGapRow> Gaps { get; set; } = new List<CoverageGapRow>();
        public bool ShowCoverage { get; set; }

        public async Task OnGetAsync(bool coverage = false)
        {
            Defs = await _context.ErpFinancialAggregateDefs
                .Include(d => d.FpaDef)
                .Include(d => d.WarehouseItem)
                .OrderBy(d => d.WarehouseItem.Name)
                .ToListAsync();

            ShowCoverage = coverage;
            if (coverage)
            {
                var aggregateNatures = new[]
                {
                    WarehouseItemNatureEnum.WarehouseItemNatureMaterial,
                    WarehouseItemNatureEnum.WarehouseItemNatureService,
                    WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset
                };

                var inUse = await _context.WarehouseItems
                    .Where(w => aggregateNatures.Contains(w.WarehouseItemNature))
                    .GroupBy(w => new { w.WarehouseItemNature, w.FpaDefId })
                    .Select(g => new
                    {
                        g.Key.WarehouseItemNature,
                        g.Key.FpaDefId,
                        ItemCount = g.Count()
                    })
                    .ToListAsync();

                var defined = Defs.Where(d => d.Active)
                    .Select(d => (d.WarehouseItemNature, d.FpaDefId))
                    .ToHashSet();

                var fpaNames = await _context.FpaKategories
                    .ToDictionaryAsync(f => f.Id, f => f.Name);

                Gaps = inUse
                    .Where(u => !defined.Contains((u.WarehouseItemNature, u.FpaDefId)))
                    .Select(u => new CoverageGapRow
                    {
                        Nature = u.WarehouseItemNature,
                        FpaDefId = u.FpaDefId,
                        FpaDefName = fpaNames.TryGetValue(u.FpaDefId, out var n) ? n : $"#{u.FpaDefId}",
                        ItemCount = u.ItemCount
                    })
                    .OrderBy(g => g.Nature).ThenBy(g => g.FpaDefName)
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var def = await _context.ErpFinancialAggregateDefs.FindAsync(id);
            if (def == null) return NotFound();

            if (def.Active)
            {
                var liveMappings = await _context.SharedItemErpMappings
                    .CountAsync(m => m.ErpId == def.WarehouseItemId);
                if (liveMappings > 0)
                {
                    TempData["Error"] = $"Δεν μπορεί να απενεργοποιηθεί: {liveMappings} αντιστοιχίσεις πελατών χρησιμοποιούν αυτό το είδος.";
                    return RedirectToPage();
                }
            }

            def.Active = !def.Active;
            def.DateLastModified = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
