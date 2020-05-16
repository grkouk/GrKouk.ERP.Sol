using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseItem WarehouseItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseItem = await _context.WarehouseItems
                .Include(m => m.BuyMeasureUnit)
                .Include(m => m.Company)
                .Include(m => m.FpaDef)
                .Include(m => m.MainMeasureUnit)
                .Include(m => m.MaterialCaterory)
                .Include(m => m.SecondaryMeasureUnit).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseItem == null)
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

            WarehouseItem = await _context.WarehouseItems.FindAsync(id);

            if (WarehouseItem != null)
            {
                _context.WarehouseItems.Remove(WarehouseItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
