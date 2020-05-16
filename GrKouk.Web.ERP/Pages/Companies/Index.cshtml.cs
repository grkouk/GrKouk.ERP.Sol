using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.Companies
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<Company> Company { get;set; }

        public async Task OnGetAsync()
        {
            Company = await _context.Companies.Include(p=>p.Currency ).ToListAsync();
        }
    }
}
