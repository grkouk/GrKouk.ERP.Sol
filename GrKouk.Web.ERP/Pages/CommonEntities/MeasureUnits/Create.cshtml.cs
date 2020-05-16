using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.CommonEntities.MeasureUnits
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public MeasureUnit MeasureUnit { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.MeasureUnits.Add(MeasureUnit);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}