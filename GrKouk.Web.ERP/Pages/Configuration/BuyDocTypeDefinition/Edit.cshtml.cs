using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocTypeDefinition
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification toastNotification;

        public EditModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        [BindProperty]
        public BuyDocTypeDef BuyDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocTypeDef = await _context.BuyDocTypeDefs
                .Include(b => b.Company)
                .Include(b=>b.TransTransactorDef)
                .Include(b => b.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocTypeDef == null)
            {
                return NotFound();
            }
            LoadCombos();
            return Page();
        }

        private void LoadCombos()
        {
           var usedPriceTypeList = Enum.GetValues(typeof(PriceTypeEnum))
                .Cast<PriceTypeEnum>()
                .Select(c => new UISelectTypeItem()
                {
                    Value = ((int)c).ToString(),
                    ValueInt = (int)c,
                    Text = c.GetDescription(),
                    Title = c.GetDescription()
                }).ToList();
            ViewData["UsedPrice"] = new SelectList(usedPriceTypeList, "Value", "Text");

            var warehouseItemNaturesList = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new UISelectTypeItem()
                {
                    ValueInt = (int)c,
                    Title = c.GetDescription()
                }).ToList();
            ViewData["warehouseItemNaturesList"] = new SelectList(warehouseItemNaturesList, "ValueInt", "Title");
            // ViewData["UsedPrice"] = new SelectList(usedPriceTypeList, "Value", "Text");
            ViewData["transactorTypesList"] =
                new SelectList(_context.TransactorTypes.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");

            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            //ViewData["TransSupplierDefId"] = new SelectList(_context.TransSupplierDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransTransactorDefId"] = new SelectList(_context.TransTransactorDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransWarehouseDefId"] = new SelectList(_context.TransWarehouseDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionList"] = new SelectList(_context.Sections.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SectionList"] = SelectListHelpers.GetSectionsList(_context);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(BuyDocTypeDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("Saved");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyDocTypeDefExists(BuyDocTypeDef.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool BuyDocTypeDefExists(int id)
        {
            return _context.BuyDocTypeDefs.Any(e => e.Id == id);
        }
    }
}
