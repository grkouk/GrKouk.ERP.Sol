using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    [Authorize(Roles = "Admin,Operator")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IDocumentTransactionService _docTransSrv;
        private readonly IToastNotification _toastNotification;

        public DeleteModel(ApiDbContext context, IDocumentTransactionService docTransSrv,IToastNotification toastNotification)
        {
            _context = context;
            _docTransSrv = docTransSrv;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public SellDocument SaleDocument { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SaleDocument = await _context.SellDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.SellDocSeries)
                .Include(b => b.SellDocType)
                .Include(b => b.Section)
                .Include(b => b.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (SaleDocument == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // const string sectionCode = "SYS-SELL-COMBINED-SCN";
            if (id == null)
            {
                return NotFound();
            }
           
            var srvResult = await _docTransSrv.DeleteBuyDocument((int)id);
            if (!srvResult.Success)
            {
                ModelState.AddModelError("", srvResult.ErrorMessage);
                _toastNotification.AddErrorToastMessage(srvResult.ErrorMessage);
                return Page();
            }
            _toastNotification.AddSuccessToastMessage("Delete operation was successfull");

            return RedirectToPage("./Index");
        }
    }
}
