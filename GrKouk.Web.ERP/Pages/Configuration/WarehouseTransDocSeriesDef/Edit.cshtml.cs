using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocSeriesDef
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransWarehouseDocSeriesDef TransWarehouseDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransWarehouseDocSeriesDef = await _context.TransWarehouseDocSeriesDefs
                .Include(t => t.Company)
                .Include(t => t.TransWarehouseDocTypeDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransWarehouseDocSeriesDef == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["TransWarehouseDocTypeDefId"] = new SelectList(_context.TransWarehouseDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(TransWarehouseDocSeriesDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransWarehouseDocSeriesDefExists(TransWarehouseDocSeriesDef.Id))
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

        private bool TransWarehouseDocSeriesDefExists(int id)
        {
            return _context.TransWarehouseDocSeriesDefs.Any(e => e.Id == id);
        }
    }
}
