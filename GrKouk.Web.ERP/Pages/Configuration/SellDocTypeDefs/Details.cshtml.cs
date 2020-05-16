using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocTypeDefs
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public SellDocTypeDef SellDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SellDocTypeDef = await _context.SellDocTypeDefs
                .Include(s => s.Company)
                .Include(s => s.TransTransactorDef)
                .Include(s => s.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (SellDocTypeDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
