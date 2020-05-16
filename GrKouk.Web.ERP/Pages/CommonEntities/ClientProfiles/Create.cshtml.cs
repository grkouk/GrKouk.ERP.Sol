using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Pages.CommonEntities.ClientProfiles
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Code");
            return Page();
        }

        [BindProperty]
        public ClientProfile ClientProfile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ClientProfiles.Add(ClientProfile);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}