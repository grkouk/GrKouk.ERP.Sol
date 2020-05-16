using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.CashRegCategories
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CashRegCategory CashRegCategory { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CashRegCategory = await _context.CashRegCategories.FirstOrDefaultAsync(m => m.Id == id);

            if (CashRegCategory == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CashRegCategory = await _context.CashRegCategories.FindAsync(id);

            if (CashRegCategory != null)
            {
                _context.CashRegCategories.Remove(CashRegCategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
