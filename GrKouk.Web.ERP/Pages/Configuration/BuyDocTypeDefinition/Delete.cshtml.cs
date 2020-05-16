using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocTypeDefinition
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BuyDocTypeDef BuyDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocTypeDef = await _context.BuyDocTypeDefs
                .Include(b => b.Company)
                .Include(b => b.TransTransactorDef)
                .Include(b => b.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocTypeDef == null)
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

            BuyDocTypeDef = await _context.BuyDocTypeDefs.FindAsync(id);

            if (BuyDocTypeDef != null)
            {
                _context.BuyDocTypeDefs.Remove(BuyDocTypeDef);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
