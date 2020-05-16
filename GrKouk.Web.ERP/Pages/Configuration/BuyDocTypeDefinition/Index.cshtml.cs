using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocTypeDefinition
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<BuyDocTypeDef> BuyDocTypeDefs { get;set; }

        public async Task OnGetAsync()
        {
            BuyDocTypeDefs = await _context.BuyDocTypeDefs
                .Include(b => b.Company)
                .Include(b => b.TransTransactorDef)
                .Include(b => b.TransWarehouseDef)
                .ToListAsync();
        }
    }
}
