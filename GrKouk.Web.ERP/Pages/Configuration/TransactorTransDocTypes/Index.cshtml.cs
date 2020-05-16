using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocTypes
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<TransTransactorDocTypeDef> TransTransactorDocTypeDef { get;set; }

        public async Task OnGetAsync()
        {
            TransTransactorDocTypeDef = await _context.TransTransactorDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransTransactorDef).ToListAsync();
        }
    }
}
