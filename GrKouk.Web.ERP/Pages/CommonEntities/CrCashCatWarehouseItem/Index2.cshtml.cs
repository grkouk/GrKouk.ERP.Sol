using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.CommonEntities.CrCashCatWarehouseItem
{
    public class IndexModel2 : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel2(ApiDbContext context)
        {
            _context = context;
        }

        public IList<CrCatWarehouseItem> CrCatWarehouseItem { get;set; }

        public async Task OnGetAsync()
        {
            CrCatWarehouseItem = await _context.CrCatWarehouseItems
                .Include(c => c.CashRegCategory)
                .Include(c => c.ClientProfile)
                .Include(c => c.WarehouseItem).ToListAsync();
        }
    }
}
