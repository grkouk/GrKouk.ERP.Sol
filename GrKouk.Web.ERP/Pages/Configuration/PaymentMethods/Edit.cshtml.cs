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
        public PaymentMethod ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
           
            ItemVm = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);

            if (ItemVm == null)
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
            ViewData["CfAccountId"] = SelectListHelpers.GetCfAccountsNoSelectionList(_context);
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            // Copy only the fields the form shows. Saving the posted object whole reset the
            // fields the form lacks (CompanyId → 0, Active → false, CreatedAt → 0001-01-01),
            // and CompanyId 0 hides the method from the cashier.
            var entity = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == ItemVm.Id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = ItemVm.Name;
            entity.Code = ItemVm.Code;
            entity.DaysOverdue = ItemVm.DaysOverdue;
            entity.AutoPayoffWay = ItemVm.AutoPayoffWay;
            entity.PayoffSeriesId = ItemVm.PayoffSeriesId;
            entity.CfAccountId = ItemVm.CfAccountId;
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private bool PaymentMethodExists(int id)
        {
            return _context.PaymentMethods.Any(e => e.Id == id);
        }
    }
}
