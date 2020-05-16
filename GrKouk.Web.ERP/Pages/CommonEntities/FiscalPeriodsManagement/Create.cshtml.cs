using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.CommonEntities.FiscalPeriodsManagement
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;

        public CreateModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public FiscalPeriod FiscalPeriod { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                string errorSummary = "";

                var modelStateErrors = this.ModelState.Values.SelectMany(m => m.Errors);
                var listOfErrors = modelStateErrors.ToList();
                foreach (var listOfError in listOfErrors)
                {
                    errorSummary += listOfError.ErrorMessage;
                }
                _toastNotification.AddErrorToastMessage(errorSummary);
                return Page();
            }

            _context.FiscalPeriods.Add(FiscalPeriod);
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Fiscal Period saved!");
            return RedirectToPage("./Index");
        }
    }
}