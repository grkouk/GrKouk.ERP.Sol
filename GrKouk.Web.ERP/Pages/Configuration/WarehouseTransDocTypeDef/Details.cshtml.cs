using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocTypeDef
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public TransWarehouseDocTypeDef TransWarehouseDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransWarehouseDocTypeDef = await _context.TransWarehouseDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransWarehouseDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransWarehouseDocTypeDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
