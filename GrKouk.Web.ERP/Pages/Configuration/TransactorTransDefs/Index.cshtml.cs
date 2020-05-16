using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDefs
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<TransTransactorDef> TransTransactorDef { get;set; }

        public async Task OnGetAsync()
        {
            TransTransactorDef = await _context.TransTransactorDefs
                .Include(t => t.Company)
                .Include(t => t.TurnOverTrans).ToListAsync();
        }
    }
}
