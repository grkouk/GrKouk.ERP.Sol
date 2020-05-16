using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Expenses
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public FinDiaryTransaction FinDiaryTransaction { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FinDiaryTransaction = await _context.FinDiaryTransactions
                .Include(f => f.Company)
                .Include(f => f.CostCentre)
                .Include(f => f.FinTransCategory)
                .Include(f => f.RevenueCentre)
                .Include(f => f.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (FinDiaryTransaction == null)
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

            FinDiaryTransaction = await _context.FinDiaryTransactions.FindAsync(id);

            if (FinDiaryTransaction != null)
            {
                _context.FinDiaryTransactions.Remove(FinDiaryTransaction);
              //  _context.FinDiaryTransactions.RemoveRange();
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
