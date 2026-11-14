using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDef
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<TransWarehouseDefListDto> ListItemsDtos { get; set; }

        public async Task OnGetAsync()
        {
            ListItemsDtos = await _context.TransWarehouseDefs
                .Select(TransWarehouseDefMappings.ToListDto)
                .ToListAsync();
        }
    }
}
