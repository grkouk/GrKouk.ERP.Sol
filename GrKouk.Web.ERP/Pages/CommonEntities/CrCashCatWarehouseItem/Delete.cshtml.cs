using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.CommonEntities.CrCashCatWarehouseItem
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CrCatWarehouseItem ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ItemVm = await _context.CrCatWarehouseItems
                .Include(c => c.CashRegCategory)
                .Include(c => c.ClientProfile)
                .Include(c => c.WarehouseItem).FirstOrDefaultAsync(m => m.Id == id);

            if (ItemVm == null)
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

            ItemVm = await _context.CrCatWarehouseItems.FindAsync(id);

            if (ItemVm != null)
            {
                _context.CrCatWarehouseItems.Remove(ItemVm);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
