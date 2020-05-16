using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public WarehouseItem WarehouseItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseItem = await _context.WarehouseItems
                .Include(m => m.BuyMeasureUnit)
                .Include(m => m.Company)
                .Include(m => m.FpaDef)
                .Include(m => m.MainMeasureUnit)
                .Include(m => m.MaterialCaterory)
                .Include(m => m.SecondaryMeasureUnit).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseItem == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
