using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocSeriesDefs
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

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
    }
}
