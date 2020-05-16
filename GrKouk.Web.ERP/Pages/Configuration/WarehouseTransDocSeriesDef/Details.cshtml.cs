using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocSeriesDef
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

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
            return Page();
        }
    }
}
