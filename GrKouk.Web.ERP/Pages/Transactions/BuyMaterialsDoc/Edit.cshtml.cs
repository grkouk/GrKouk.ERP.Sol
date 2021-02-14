using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.BuyMaterialsDoc
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public string SeekType { get; set; }
        public bool InitialLoad = true;

        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
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

       

        private bool BuyMaterialsDocumentExists(int id)
        {
            return _context.BuyDocuments.Any(e => e.Id == id);
        }
    }
}
