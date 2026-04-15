using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.ErpFinancialAggregates
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ErpFinancialAggregateDef ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ItemVm = await _context.ErpFinancialAggregateDefs.FindAsync(id);
            if (ItemVm == null) return NotFound();
            LoadCombos();
            return Page();
        }

        private void LoadCombos()
        {
            var aggregateNatures = new[]
            {
                WarehouseItemNatureEnum.WarehouseItemNatureMaterial,
                WarehouseItemNatureEnum.WarehouseItemNatureService,
                WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset
            };
            ViewData["NatureList"] = new SelectList(
                aggregateNatures.Select(n => new { Value = (int)n, Text = n.GetDescription() }),
                "Value", "Text");
            ViewData["FpaDefList"] = new SelectList(
                _context.FpaKategories.OrderBy(f => f.Name).ToList(),
                "Id", "Name");
            ViewData["WarehouseItemList"] = new SelectList(
                _context.WarehouseItems
                    .Where(w => w.WarehouseItemNature == ItemVm.WarehouseItemNature
                                && w.FpaDefId == ItemVm.FpaDefId)
                    .OrderBy(w => w.Name)
                    .Select(w => new { w.Id, Text = w.Name + " {" + w.Code + "}" })
                    .ToList(),
                "Id", "Text");
        }

        public async Task<IActionResult> OnGetWarehouseItemsAsync(int nature, int fpaDefId)
        {
            var natureEnum = (WarehouseItemNatureEnum)nature;
            var items = await _context.WarehouseItems
                .Where(w => w.WarehouseItemNature == natureEnum && w.FpaDefId == fpaDefId)
                .OrderBy(w => w.Name)
                .Select(w => new { id = w.Id, text = $"{w.Name} {{{w.Code}}}" })
                .ToListAsync();
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var existing = await _context.ErpFinancialAggregateDefs.FindAsync(ItemVm.Id);
            if (existing == null) return NotFound();

            var item = await _context.WarehouseItems.FindAsync(ItemVm.WarehouseItemId);
            if (item == null)
                ModelState.AddModelError(nameof(ItemVm.WarehouseItemId), "Άγνωστο WarehouseItem.");
            else
            {
                if (item.WarehouseItemNature != ItemVm.WarehouseItemNature)
                    ModelState.AddModelError(nameof(ItemVm.WarehouseItemId), "Η φύση του επιλεγμένου είδους δεν ταιριάζει.");
                if (item.FpaDefId != ItemVm.FpaDefId)
                    ModelState.AddModelError(nameof(ItemVm.WarehouseItemId), "Το ΦΠΑ του επιλεγμένου είδους δεν ταιριάζει.");
            }

            var duplicate = await _context.ErpFinancialAggregateDefs
                .AnyAsync(d => d.Id != ItemVm.Id
                               && d.WarehouseItemNature == ItemVm.WarehouseItemNature
                               && d.FpaDefId == ItemVm.FpaDefId);
            if (duplicate)
                ModelState.AddModelError(string.Empty, "Υπάρχει ήδη άλλος ορισμός για αυτόν τον συνδυασμό.");

            // If WarehouseItemId is changing, block when live mappings point at the old one.
            if (existing.WarehouseItemId != ItemVm.WarehouseItemId)
            {
                var liveMappings = await _context.SharedItemErpMappings
                    .CountAsync(m => m.ErpId == existing.WarehouseItemId);
                if (liveMappings > 0)
                    ModelState.AddModelError(string.Empty,
                        $"Δεν μπορεί να αλλάξει το είδος: {liveMappings} αντιστοιχίσεις πελατών χρησιμοποιούν το τρέχον είδος.");
            }

            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            existing.WarehouseItemNature = ItemVm.WarehouseItemNature;
            existing.FpaDefId = ItemVm.FpaDefId;
            existing.WarehouseItemId = ItemVm.WarehouseItemId;
            existing.Active = ItemVm.Active;
            existing.Notes = ItemVm.Notes;
            existing.DateLastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
