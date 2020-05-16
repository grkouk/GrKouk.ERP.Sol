using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDocTypeDef
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<TransWarehouseDocTypeDef> TransWarehouseDocTypeDef { get;set; }

        public async Task OnGetAsync()
        {
            TransWarehouseDocTypeDef = await _context.TransWarehouseDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransWarehouseDef).ToListAsync();
        }
    }
}
