using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FpaKategories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<FpaDef> FpaDef { get;set; }

        public async Task OnGetAsync()
        {
            FpaDef = await _context.FpaKategories.ToListAsync();
        }
    }
}
