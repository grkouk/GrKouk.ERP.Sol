using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.CashRegCategories
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<CashRegCategory> CashRegCategory { get;set; }

        public async Task OnGetAsync()
        {
            CashRegCategory = await _context.CashRegCategories.ToListAsync();
        }
    }
}
