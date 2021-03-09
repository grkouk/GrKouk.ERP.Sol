using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocTypes
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
        public TransTransactorDocTypeDef ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TransTransactorDocTypeDefs.Add(ItemVm);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        private void LoadCombos()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["TransTransactorDefId"] = new SelectList(_context.TransTransactorDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SectionList"] = SelectListHelpers.GetSectionsList(_context);
            var transactorTypesListJs = _context.TransactorTypes.OrderBy(p => p.Name)
                           .Select(p => new DiaryDocTypeItem()
                           {
                               Title = p.Name,
                               Value = p.Id
                           }).ToList();
            ViewData["TransactorTypesListJs"] = transactorTypesListJs;
            ViewData["DefaultCfaId"] = SelectListHelpers.GetCfAccountsNoSelectionList(_context);
        }
    }
}