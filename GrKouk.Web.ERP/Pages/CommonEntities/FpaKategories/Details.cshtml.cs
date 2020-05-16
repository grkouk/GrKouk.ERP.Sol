using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FpaKategories
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public FpaDef FpaDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FpaDef = await _context.FpaKategories.FirstOrDefaultAsync(m => m.Id == id);

            if (FpaDef == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
