using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.WarehouseTransMng
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public WarehouseTransaction WarehouseTransaction { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransaction = await _context.WarehouseTransactions
                .Include(w => w.Company)
                .Include(w => w.FiscalPeriod)
                .Include(w => w.WarehouseItem)
                .Include(w => w.Section)
                .Include(w => w.TransWarehouseDocSeries)
                .Include(w => w.TransWarehouseDocType).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseTransaction == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
