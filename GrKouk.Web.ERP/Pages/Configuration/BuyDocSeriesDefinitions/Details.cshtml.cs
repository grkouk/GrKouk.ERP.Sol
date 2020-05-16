using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocSeriesDefinitions
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public BuyDocSeriesDef BuyDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocSeriesDef = await _context.BuyDocSeriesDefs
                .Include(b => b.BuyDocTypeDef)
                .Include(b => b.Company).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocSeriesDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
