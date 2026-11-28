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
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
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
        [BindProperty]
        public PaymentMethod ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            // The form has no company: a new method belongs to all companies ({Ολες}), like every
            // existing one. CompanyId 0 hid it from the cashier's list (company or all companies).
            var allCompaniesCode = await _context.AppSettings
                .Where(p => p.Code == GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey)
                .Select(p => p.Value)
                .SingleOrDefaultAsync();
            var allCompaniesId = await _context.Companies
                .Where(c => c.Code == allCompaniesCode)
                .Select(c => c.Id)
                .SingleOrDefaultAsync();
            if (allCompaniesId == 0)
            {
                ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η εταιρεία «Όλες» (ρύθμιση ALLCOMPCODE).");
                LoadCombos();
                return Page();
            }

            ItemVm.CompanyId = allCompaniesId;
            ItemVm.Active = true;
            ItemVm.CreatedAt = DateTime.UtcNow;
            ItemVm.ModifiedAt = DateTime.UtcNow;

            _context.PaymentMethods.Add(ItemVm);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}