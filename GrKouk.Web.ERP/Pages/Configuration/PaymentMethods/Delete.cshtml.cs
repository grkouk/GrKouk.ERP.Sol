using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PaymentMethod PaymentMethod { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PaymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);

            if (PaymentMethod == null)
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

            PaymentMethod = await _context.PaymentMethods.FindAsync(id);

            // Buy and sell documents reference the method with cascade delete: never delete a used one.
            if (PaymentMethod != null
                && (await PaymentMethodUsage.GetUsedCodesAsync(_context, new[] { PaymentMethod.Id })).Count > 0)
            {
                ModelState.AddModelError(string.Empty, "Ο τρόπος πληρωμής χρησιμοποιείται σε παραστατικά και δεν μπορεί να διαγραφεί.");
                return Page();
            }

            if (PaymentMethod != null)
            {
                _context.PaymentMethods.Remove(PaymentMethod);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./IndexSf");
        }
    }
}
