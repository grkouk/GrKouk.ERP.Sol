using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.ClientProfiles
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<ClientProfile> ClientProfile { get;set; }

        public async Task OnGetAsync()
        {
            ClientProfile = await _context.ClientProfiles
                .Include(c => c.Company).ToListAsync();
        }
    }
}
