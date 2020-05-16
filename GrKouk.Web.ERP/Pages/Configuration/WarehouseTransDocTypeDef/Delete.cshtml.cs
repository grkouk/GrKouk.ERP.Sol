using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocTypeDef
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransWarehouseDocTypeDef TransWarehouseDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransWarehouseDocTypeDef = await _context.TransWarehouseDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransWarehouseDocTypeDef == null)
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

            TransWarehouseDocTypeDef = await _context.TransWarehouseDocTypeDefs.FindAsync(id);

            if (TransWarehouseDocTypeDef != null)
            {
                _context.TransWarehouseDocTypeDefs.Remove(TransWarehouseDocTypeDef);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
