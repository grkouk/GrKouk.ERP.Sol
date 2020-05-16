using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTypes
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransactorType TransactorType { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransactorType = await _context.TransactorTypes.FirstOrDefaultAsync(m => m.Id == id);

            if (TransactorType == null)
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

            TransactorType = await _context.TransactorTypes.FindAsync(id);

            if (TransactorType != null)
            {
                _context.TransactorTypes.Remove(TransactorType);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
