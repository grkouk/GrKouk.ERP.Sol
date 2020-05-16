using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDefs
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public TransTransactorDef TransTransactorDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransTransactorDef = await _context.TransTransactorDefs
                .Include(t => t.Company)
                .Include(t => t.TurnOverTrans).FirstOrDefaultAsync(m => m.Id == id);

            if (TransTransactorDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
