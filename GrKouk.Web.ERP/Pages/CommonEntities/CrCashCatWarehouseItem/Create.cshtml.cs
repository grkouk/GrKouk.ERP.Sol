using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.CrCashCatWarehouseItem
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
           

            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["CashRegCategoryId"] = new SelectList(_context.CashRegCategories.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["ClientProfileId"] = new SelectList(_context.ClientProfiles.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["WarehouseItemId"] = new SelectList(_context.WarehouseItems.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
          
        }
        [BindProperty]
        public CrCatWarehouseItem ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.CrCatWarehouseItems.Add(ItemVm);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}