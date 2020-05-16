using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.CommonEntities.CrCashCatWarehouseItem
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CrCatWarehouseItem ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ItemVm = await _context.CrCatWarehouseItems
                .Include(c => c.CashRegCategory)
                .Include(c => c.ClientProfile)
                .Include(c => c.WarehouseItem).FirstOrDefaultAsync(m => m.Id == id);

            if (ItemVm == null)
            {
                return NotFound();
            }
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ItemVm).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CrCatWarehouseItemExists(ItemVm.Id))
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
        private void LoadCombos()
        {

            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["CashRegCategoryId"] = new SelectList(_context.CashRegCategories.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["ClientProfileId"] = new SelectList(_context.ClientProfiles.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["WarehouseItemId"] = new SelectList(_context.WarehouseItems.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
           
        }
        private bool CrCatWarehouseItemExists(int id)
        {
            return _context.CrCatWarehouseItems.Any(e => e.Id == id);
        }
    }
}
