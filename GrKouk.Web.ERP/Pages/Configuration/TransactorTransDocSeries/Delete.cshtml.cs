using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocSeries
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransTransactorDocSeriesDef TransTransactorDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransTransactorDocSeriesDef = await _context.TransTransactorDocSeriesDefs
                .Include(t => t.Company)
                .Include(t => t.TransTransactorDocTypeDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransTransactorDocSeriesDef == null)
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

            TransTransactorDocSeriesDef = await _context.TransTransactorDocSeriesDefs.FindAsync(id);

            if (TransTransactorDocSeriesDef != null)
            {
                _context.TransTransactorDocSeriesDefs.Remove(TransTransactorDocSeriesDef);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
