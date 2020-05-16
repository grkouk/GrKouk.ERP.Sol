using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FiscalPeriodsManagement
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public FiscalPeriod FiscalPeriod { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(m => m.Id == id);

            if (FiscalPeriod == null)
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

            FiscalPeriod = await _context.FiscalPeriods.FindAsync(id);

            if (FiscalPeriod != null)
            {
                _context.FiscalPeriods.Remove(FiscalPeriod);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
