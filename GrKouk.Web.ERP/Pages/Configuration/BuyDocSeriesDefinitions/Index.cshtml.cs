using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocSeriesDefinitions
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<BuyDocSeriesDef> BuyMaterialDocSeriesDef { get;set; }

        public async Task OnGetAsync()
        {
            BuyMaterialDocSeriesDef = await _context.BuyDocSeriesDefs
                .Include(b => b.BuyDocTypeDef)
                .Include(b => b.Company).ToListAsync();
        }
    }
}
