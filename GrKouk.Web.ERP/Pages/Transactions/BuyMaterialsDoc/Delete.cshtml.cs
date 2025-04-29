using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.BuyMaterialsDoc
{
    [Authorize(Roles = "Admin, Operator")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDocumentTransactionService _docTransSrv;
        private readonly IToastNotification _toastNotification;

        public DeleteModel(ApiDbContext context, IMapper mapper, IDocumentTransactionService docTransSrv,IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _docTransSrv = docTransSrv;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public BuyDocModifyDto ItemVm { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var buyMatDoc = await _context.BuyDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.BuyDocSeries)
                .Include(b => b.BuyDocType)
                .Include(b => b.Section)

                .Include(b => b.Transactor)
                .Include(b => b.BuyDocLines)
                .ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            ItemVm = _mapper.Map<BuyDocModifyDto>(buyMatDoc);

            if (ItemVm == null)
            {
                return NotFound();
            }
            //check for new values or old values
            var vmLines = ItemVm.BuyDocLines;
            foreach (var vmLine in vmLines)
            {
                if (vmLine.TransactionUnitId==0)
                {
                    vmLine.TransactionUnitId = vmLine.PrimaryUnitId;
                    vmLine.TransactionUnitFactor = 1;
                    vmLine.TransUnitPrice = vmLine.UnitPrice;
                    vmLine.TransactionQuantity = vmLine.Quontity1;
                }

            }
            
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // const string sectionCode = "SYS-BUY-MATERIALS-SCN";
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
        private void LoadCombos()
        {
            List<SelectListItem> seekTypes = new List<SelectListItem>
            {
                new SelectListItem() {Value = "NAME", Text = "Name"},
                new SelectListItem() {Value ="CODE", Text = "Code"},
                new SelectListItem() {Value = "BARCODE", Text = "Barcode"}
            };
            ViewData["SeekType"] = new SelectList(seekTypes, "Value", "Text");

            var supplierList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.SUPPLIER").OrderBy(s => s.Name).AsNoTracking();
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["BuyDocSeriesId"] = new SelectList(_context.BuyDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(supplierList, "Id", "Name");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
        }
    }
}
