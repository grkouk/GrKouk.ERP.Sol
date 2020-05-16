using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocSeries
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<TransTransactorDocSeriesDef> TransTransactorDocSeriesDef { get;set; }

        public async Task OnGetAsync()
        {
            TransTransactorDocSeriesDef = await _context.TransTransactorDocSeriesDefs
                .Include(t => t.Company)
                .Include(t => t.TransTransactorDocTypeDef).ToListAsync();
        }
    }
}
