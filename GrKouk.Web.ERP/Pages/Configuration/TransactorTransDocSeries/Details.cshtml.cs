using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocSeries
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

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
    }
}
