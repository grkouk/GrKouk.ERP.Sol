using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.MaterialCodes
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
        public WrItemCode WarehouseItemCode { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
           

            WarehouseItemCode = await _context.WrItemCodes
                .Include(m => m.WarehouseItem).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseItemCode == null)
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

            _context.Attach(WarehouseItemCode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialCodeExists(WarehouseItemCode.Id))
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

        private bool MaterialCodeExists(int id)
        {
            return _context.WrItemCodes.Any(e => e.Id == id);
        }
    }
}
