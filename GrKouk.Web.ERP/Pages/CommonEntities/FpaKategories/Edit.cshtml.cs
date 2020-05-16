using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FpaKategories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(FpaDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FpaDefExists(FpaDef.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool FpaDefExists(int id)
        {
            return _context.FpaKategories.Any(e => e.Id == id);
        }
    }
}
