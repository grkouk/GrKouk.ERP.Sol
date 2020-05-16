using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FiscalPeriodsManagement
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<FiscalPeriod> FiscalPeriod { get;set; }

        public async Task OnGetAsync()
        {
            FiscalPeriod = await _context.FiscalPeriods.ToListAsync();
        }
    }
}
