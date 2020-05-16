using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocTypeDefinition
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public BuyDocTypeDef BuyDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocTypeDef = await _context.BuyDocTypeDefs
                .Include(b => b.Company)
                .Include(b => b.TransTransactorDef)
                .Include(b => b.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocTypeDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
