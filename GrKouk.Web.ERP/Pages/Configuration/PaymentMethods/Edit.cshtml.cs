using System;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.PaymentMethods;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PaymentMethodModifyDto ItemVm { get; set; }

        public PaymentMethodForm Form { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            ItemVm = entity.ToModifyDto();
            Form = await PaymentMethodForm.LoadAsync(_context);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var entity = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == ItemVm.Id);
            if (entity == null)
            {
                return NotFound();
            }

            await PaymentMethodForm.ValidateAsync(_context, ModelState, ItemVm.Id, ItemVm.Code, ItemVm.CompanyId);
            if (!ModelState.IsValid)
            {
                // The dates are shown, not posted: show the stored ones again.
                ItemVm.CreatedAt = entity.CreatedAt;
                ItemVm.ModifiedAt = entity.ModifiedAt;
                Form = await PaymentMethodForm.LoadAsync(_context);
                return Page();
            }

            // Copy the form's fields onto the stored record; Id and CreatedAt stay as they are.
            ItemVm.ApplyTo(entity);
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage("./IndexSf");
        }
    }
}
