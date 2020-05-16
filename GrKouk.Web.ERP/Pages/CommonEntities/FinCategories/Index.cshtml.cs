using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FinCategories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<FinTransCategory> FinTransCategory { get;set; }

        public async Task OnGetAsync()
        {
            FinTransCategory = await _context.FinTransCategories.ToListAsync();
        }
    }
}
