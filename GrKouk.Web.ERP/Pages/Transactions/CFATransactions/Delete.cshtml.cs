using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.CFATransactions
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        public bool NotUpdatable;
        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }
        public int CfaId { get; set; }
        [BindProperty]
        public CfaTransactionModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var transactionToModify = await _context.CashFlowAccountTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.DocumentSeries)
                .Include(t => t.DocumentType)
                .Include(t => t.CashFlowAccount)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null)
            {
                return NotFound();
            }
            ItemVm = transactionToModify.ToModifyDto();
            NotUpdatable = ItemVm.CreatorId != 0;
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemToDelete = await _context.CashFlowAccountTransactions.FindAsync(id);

            if (itemToDelete != null)
            {
                _context.CashFlowAccountTransactions.Remove(itemToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
        private void LoadCombos()
        {
            var companiesList = FiltersHelper.GetSolidCompaniesFilterList(_context);
            ViewData["CompanyId"] = new SelectList(companiesList, "Value", "Text");
           
            ViewData["DocSeriesId"] =
                new SelectList(_context.CashFlowDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            ViewData["CashFlowAccountId"] =
                new SelectList(_context.CashFlowAccounts.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
        }
    }
}
