using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.Diaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocTypeDefs
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

        [BindProperty]
        public SellDocTypeDef SellDocTypeDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.SellDocTypeDefs.Add(SellDocTypeDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void LoadCombos()
        {
            List<SelectListItem> usedPriceTypeList = new List<SelectListItem>
            {

                new SelectListItem() {Value = PriceTypeEnum.PriceTypeEnumNetto.ToString(), Text = "Καθαρή Τιμή"},
                new SelectListItem() {Value = PriceTypeEnum.PriceTypeEnumBrutto.ToString(), Text = "Μικτή Τιμή"}
               
            };
            var warehouseItemNaturesList = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new UISelectTypeItem()
                {
                    ValueInt = (int) c,
                    Title = c.GetDescription()
                }).ToList();
            ViewData["warehouseItemNaturesList"] = new SelectList(warehouseItemNaturesList, "ValueInt", "Title");
            ViewData["transactorTypesList"] =
                new SelectList(_context.TransactorTypes.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");

            ViewData["UsedPrice"] = new SelectList(usedPriceTypeList, "Value", "Text");
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");

            ViewData["TransTransactorDefId"] = new SelectList(_context.TransTransactorDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransWarehouseDefId"] = new SelectList(_context.TransWarehouseDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionList"] = new SelectList(_context.Sections.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SectionList"] = SelectListHelpers.GetSectionsList(_context);
        }
    }
}