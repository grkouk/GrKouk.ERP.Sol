using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.SectionsManagement
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<Section> Section { get;set; }

        public async Task OnGetAsync()
        {
            Section = await _context.Sections.ToListAsync();
        }
    }
}
