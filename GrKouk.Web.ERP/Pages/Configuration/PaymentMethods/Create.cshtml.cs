using System;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.PaymentMethods;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PaymentMethodCreateDto ItemVm { get; set; }

        public PaymentMethodForm Form { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // A new method is active and, by default, belongs to all companies ({Ολες}):
            // the cash register lists the methods of its company or of all companies.
            ItemVm = new PaymentMethodCreateDto
            {
                Active = true,
                CompanyId = await PaymentMethodForm.GetAllCompaniesIdAsync(_context)
            };
            Form = await PaymentMethodForm.LoadAsync(_context);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PaymentMethodForm.ValidateAsync(_context, ModelState, 0, ItemVm.Code, ItemVm.CompanyId);
            if (!ModelState.IsValid)
            {
                Form = await PaymentMethodForm.LoadAsync(_context);
                return Page();
            }

            var entity = ItemVm.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;

            _context.PaymentMethods.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./IndexSf");
        }
    }
}
