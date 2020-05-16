using System.Threading.Tasks;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MediaMng
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MediaEntry MediaEntry { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MediaEntry = await _context.MediaEntries.FirstOrDefaultAsync(m => m.Id == id);

            if (MediaEntry == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MediaEntry = await _context.MediaEntries.FindAsync(id);

            if (MediaEntry != null)
            {
                _context.MediaEntries.Remove(MediaEntry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
