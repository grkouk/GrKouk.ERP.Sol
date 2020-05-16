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
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
        ViewData["TransWarehouseDocTypeDefId"] = new SelectList(_context.TransWarehouseDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public TransWarehouseDocSeriesDef TransWarehouseDocSeriesDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TransWarehouseDocSeriesDefs.Add(TransWarehouseDocSeriesDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}