using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.ERP.Pages.CommonEntities.CrCashCatWarehouseItem
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public CrCatWarehouseItem CrCatWarehouseItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CrCatWarehouseItem = await _context.CrCatWarehouseItems
                .Include(c => c.CashRegCategory)
                .Include(c => c.ClientProfile)
                .Include(c => c.WarehouseItem).FirstOrDefaultAsync(m => m.Id == id);

            if (CrCatWarehouseItem == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
