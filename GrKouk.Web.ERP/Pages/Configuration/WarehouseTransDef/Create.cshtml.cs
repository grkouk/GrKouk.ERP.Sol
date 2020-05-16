using System;
using System.Collections.Generic;
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

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDef
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            LoadCombos();
            return Page();
        }

        private void LoadCombos()
        {
            // var fMovements = _context.FinancialMovements.AsNoTracking().ToList();

            var inventoryActions = Enum.GetValues(typeof(InventoryActionEnum))
                .Cast<InventoryActionEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();

            var inventoryValueActions = Enum.GetValues(typeof(InventoryValueActionEnum))
                .Cast<InventoryValueActionEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();

            //var infoEntityActionList = Enum.GetValues(typeof(InfoEntityActionEnum))
            //    .Cast<InfoEntityActionEnum>()
            //    .Select(c => new SelectListItem()
            //    {
            //        Value = c.ToString(),
            //        Text = c.GetDescription()
            //    }).ToList();

            ViewData["MaterialInventoryActions"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["MaterialInventoryValueActions"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["MaterialInvoicedVolumeAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["MaterialInvoicedValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["ServiceInventoryAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["ServiceInventoryValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["ExpenseInventoryAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["ExpenseInventoryValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["IncomeInventoryAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["IncomeInventoryValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["FixedAssetInventoryAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["FixedAssetInventoryValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");

            ViewData["RawMaterialInventoryAction"] = new SelectList(inventoryActions, "Value", "Text");
            ViewData["RawMaterialInventoryValueAction"] = new SelectList(inventoryValueActions, "Value", "Text");


            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p=>p.Code).AsNoTracking(), "Id", "Code");
            var dbSeriesList = _context.TransWarehouseDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> seriesList = new List<SelectListItem>();
            seriesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{No Default series}" });
            foreach (var dbSeriesItem in dbSeriesList)
            {
                seriesList.Add(new SelectListItem() { Value = dbSeriesItem.Id.ToString(), Text = dbSeriesItem.Name });
            }
            ViewData["DefaultDocSeriesId"] = new SelectList(seriesList, "Value", "Text");
        }

        [BindProperty]
        public TransWarehouseDef TransWarehouseDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TransWarehouseDefs.Add(TransWarehouseDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}