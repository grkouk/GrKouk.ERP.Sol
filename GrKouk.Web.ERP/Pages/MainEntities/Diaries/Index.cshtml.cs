using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Diaries
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<DiaryDef> DiaryDef { get;set; }

        public async Task OnGetAsync()
        {
            DiaryDef = await _context.DiaryDefs.ToListAsync();
        }
    }
}
