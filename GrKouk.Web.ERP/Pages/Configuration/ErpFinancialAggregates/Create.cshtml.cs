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
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ErpFinancialAggregateDef ItemVm { get; set; } = new();

        public IActionResult OnGet()
        {
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
            var aggregateNatures = new[]
            {
                WarehouseItemNatureEnum.WarehouseItemNatureMaterial,
                WarehouseItemNatureEnum.WarehouseItemNatureService,
                WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset
            };
            if (!aggregateNatures.Contains(ItemVm.WarehouseItemNature))
                ModelState.AddModelError(nameof(ItemVm.WarehouseItemNature), "Μόνο Εμπόρευμα/Υπηρεσία/Πάγιο επιτρέπονται.");

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
                .AnyAsync(d => d.WarehouseItemNature == ItemVm.WarehouseItemNature
                               && d.FpaDefId == ItemVm.FpaDefId);
            if (duplicate)
                ModelState.AddModelError(string.Empty, "Υπάρχει ήδη ορισμός για αυτόν τον συνδυασμό (Φύση, ΦΠΑ).");

            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            ItemVm.DateCreated = DateTime.UtcNow;
            ItemVm.DateLastModified = DateTime.UtcNow;
            _context.ErpFinancialAggregateDefs.Add(ItemVm);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
