using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocTypeDef
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
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
           ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
           ViewData["TransWarehouseDefId"] = new SelectList(_context.TransWarehouseDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(TransWarehouseDocTypeDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransWarehouseDocTypeDefExists(TransWarehouseDocTypeDef.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool TransWarehouseDocTypeDefExists(int id)
        {
            return _context.TransWarehouseDocTypeDefs.Any(e => e.Id == id);
        }
    }
}
