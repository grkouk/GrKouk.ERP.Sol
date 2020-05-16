using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;

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
            LoadCombos();
            return Page();
        }
        private void LoadCombos()
        {
            var seriesAutoPayOffList = Enum.GetValues(typeof(SeriesAutoPayoffEnum))
                .Cast<SeriesAutoPayoffEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();
            ViewData["AutoPayoffWay"] = new SelectList(seriesAutoPayOffList, "Value", "Text");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PaymentMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentMethodExists(PaymentMethod.Id))
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

        private bool PaymentMethodExists(int id)
        {
            return _context.PaymentMethods.Any(e => e.Id == id);
        }
    }
}
