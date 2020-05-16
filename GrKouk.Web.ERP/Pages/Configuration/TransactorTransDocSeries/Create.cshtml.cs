using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocSeries
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
       LoadCombos();
            return Page();
        }

        [BindProperty]
        public TransTransactorDocSeriesDef TransTransactorDocSeriesDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TransTransactorDocSeriesDefs.Add(TransTransactorDocSeriesDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        private void LoadCombos()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["TransTransactorDocTypeDefId"] = new SelectList(_context.TransTransactorDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
        }
    }
}