using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Diaries
{
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public DiaryDef DiaryDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DiaryDef = await _context.DiaryDefs.FirstOrDefaultAsync(m => m.Id == id);

            if (DiaryDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
