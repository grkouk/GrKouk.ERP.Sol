using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocTypes
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransTransactorDocTypeDef TransTransactorDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransTransactorDocTypeDef = await _context.TransTransactorDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransTransactorDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransTransactorDocTypeDef == null)
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

            TransTransactorDocTypeDef = await _context.TransTransactorDocTypeDefs.FindAsync(id);

            if (TransTransactorDocTypeDef != null)
            {
                _context.TransTransactorDocTypeDefs.Remove(TransTransactorDocTypeDef);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
