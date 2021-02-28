using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Configuration.CashFlow.CFADocSeriesDefs
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
        public CashFlowDocSeriesDef ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _context.CashFlowDocSeriesDefs.AddAsync(ItemVm);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void LoadCombos()
        {
           ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
          
            ViewData["CashFlowDocTypeDefId"] =
                new SelectList(_context.CashFlowDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
        }
    }
}