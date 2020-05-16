using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.MaterialCategories
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public MaterialCategory MaterialCategory { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MaterialCategory = await _context.MaterialCategories.FirstOrDefaultAsync(m => m.Id == id);

            if (MaterialCategory == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
