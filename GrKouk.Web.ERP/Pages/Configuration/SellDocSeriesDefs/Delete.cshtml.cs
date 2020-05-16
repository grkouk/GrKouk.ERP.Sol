using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocSeriesDefs
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SellDocSeriesDef SellDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SellDocSeriesDef = await _context.SellDocSeriesDefs
                .Include(s => s.Company)
                .Include(s => s.SellDocTypeDef).FirstOrDefaultAsync(m => m.Id == id);

            if (SellDocSeriesDef == null)
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

            SellDocSeriesDef = await _context.SellDocSeriesDefs.FindAsync(id);

            if (SellDocSeriesDef != null)
            {
                _context.SellDocSeriesDefs.Remove(SellDocSeriesDef);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
