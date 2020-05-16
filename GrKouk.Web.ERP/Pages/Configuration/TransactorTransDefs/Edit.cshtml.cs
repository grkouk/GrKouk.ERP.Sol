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

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDefs
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransTransactorDef TransTransactorDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransTransactorDef = await _context.TransTransactorDefs
                .Include(t => t.Company)
                .Include(t => t.TurnOverTrans).FirstOrDefaultAsync(m => m.Id == id);

            if (TransTransactorDef == null)
            {
                return NotFound();
            }
          LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(TransTransactorDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransTransactorDefExists(TransTransactorDef.Id))
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

        private bool TransTransactorDefExists(int id)
        {
            return _context.TransTransactorDefs.Any(e => e.Id == id);
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
