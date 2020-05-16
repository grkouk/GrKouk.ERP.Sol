using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.RecurringTransactions
{
    public class Delete : PageModel
    {
        private readonly ApiDbContext _context;

        public Delete(ApiDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public RecurringTransDoc ItemVm { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ItemVm = await _context.RecurringTransDocs
                .Include(b => b.Company)
                .Include(b => b.Transactor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ItemVm == null)
            {
                return NotFound();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            //const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            if (id == null)
            {
                return NotFound();
            }
            // #region Section Management
            //
            // var section = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == sectionCode);
            // if (section == null)
            // {
            //    
            //    
            //     return NotFound(new
            //     {
            //         error = "Could not locate section "
            //     });
            // }
            //
            // #endregion
            ItemVm = await _context.RecurringTransDocs.FindAsync(id);

            if (ItemVm != null)
            {

                _context.RecurringTransDocLines.RemoveRange(_context.RecurringTransDocLines.Where(p => p.RecurringTransDocId == id));

                _context.RecurringTransDocs.Remove(ItemVm);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}