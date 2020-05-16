using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDefs
{
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
       LoadCombos();
            return Page();
        }

        [BindProperty]
        public TransTransactorDef TransTransactorDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TransTransactorDefs.Add(TransTransactorDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void LoadCombos()
        {
            List<SelectListItem> financialActions = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Value = FinActionsEnum.FinActionsEnumNoChange.ToString(),
                    Text = "No Change"
                },
               new SelectListItem()
                {
                    Value = FinActionsEnum.FinActionsEnumDebit.ToString(),
                    Text = "Debit"
                },
               new SelectListItem()
               {
                   Value = FinActionsEnum.FinActionsEnumCredit.ToString(),
                   Text = "Credit"
               },

                new SelectListItem()
                {
                    Value = FinActionsEnum.FinActionsEnumNegativeDebit.ToString(),
                    Text = "Neg.Debit"
                },
                new SelectListItem()
                {
                    Value = FinActionsEnum.FinActionsEnumNegativeCredit.ToString(),
                    Text = "Neg.Credit"
                }
            };
            ViewData["FinancialActions"] = new SelectList(financialActions, "Value", "Text");
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["TurnOverTransId"] = new SelectList(_context.FinancialMovements.AsNoTracking(), "Id", "Name");

            var dbSeriesList = _context.TransTransactorDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> seriesList = new List<SelectListItem>();
            seriesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{No Default series}" });
            foreach (var dbSeriesItem in dbSeriesList)
            {
                seriesList.Add(new SelectListItem() { Value = dbSeriesItem.Id.ToString(), Text = dbSeriesItem.Name });
            }
            ViewData["DefaultDocSeriesId"] = new SelectList(seriesList, "Value", "Text");
        }
    }
}