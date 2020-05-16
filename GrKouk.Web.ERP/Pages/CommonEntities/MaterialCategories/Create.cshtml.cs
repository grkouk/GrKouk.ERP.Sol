using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.CommonEntities.MaterialCategories
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
        public MaterialCategory MaterialCategory { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.MaterialCategories.Add(MaterialCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}