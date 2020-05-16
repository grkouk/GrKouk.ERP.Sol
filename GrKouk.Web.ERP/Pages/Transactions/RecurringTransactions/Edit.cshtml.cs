using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.RecurringTransactions
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
        public RecurringDocModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var buyMatDoc = await _context.RecurringTransDocs
                .Include(b => b.Company)
                .Include(b => b.Transactor)
                .Include(b => b.DocLines)
                .ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            ItemVm = _mapper.Map<RecurringDocModifyDto>(buyMatDoc);

            if (ItemVm == null)
            {
                return NotFound();
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
            var docTypes = Enum.GetValues(typeof(RecurringDocTypeEnum))
               .Cast<RecurringDocTypeEnum>()
               .Select(c => new SelectListItem()
               {
                   Value = c.ToString(),
                   Text = c.GetDescription()
               }).ToList();
            ViewData["DocType"] = new SelectList(docTypes, "Value", "Text");
            ViewData["RecurringFrequency"] = FiltersHelper.GetRecurringFrequencyList();
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            IQueryable transactorList;
            IQueryable docTypeList;
            switch (ItemVm.RecurringDocType)
            {
                case RecurringDocTypeEnum.BuyType:
                    transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.SUPPLIER").OrderBy(s => s.Name).AsNoTracking();
                    docTypeList = _context.BuyDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
                    ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");
                    ViewData["DocSeriesId"] = new SelectList(docTypeList, "Id", "Name");
                    break;
                case RecurringDocTypeEnum.SellType:
                    transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.CUSTOMER" || s.TransactorType.Code == "SYS.DEPARTMENT").OrderBy(s => s.Name).AsNoTracking();
                    docTypeList = _context.SellDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
                    ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");
                    ViewData["DocSeriesId"] = new SelectList(docTypeList, "Id", "Name");
                    break;
                default:
                    break;
            }


            var buyDocSeriesListJs = _context.BuyDocSeriesDefs.OrderBy(p => p.Name)
               .Select(p => new SelectListItem()
               {
                   Text = p.Name,
                   Value = p.Id.ToString()
               }).ToList();
            var sellDocSeriesListJs = _context.SellDocSeriesDefs.OrderBy(p => p.Name)
              .Select(p => new SelectListItem()
              {
                  Text = p.Name,
                  Value = p.Id.ToString()
              }).ToList();
            var supplierListJs = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.SUPPLIER").OrderBy(s => s.Name)
              .Select(p => new SelectListItem()
              {
                  Text = p.Name,
                  Value = p.Id.ToString()
              }).ToList();
            var customerListJs = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.CUSTOMER" || s.TransactorType.Code == "SYS.DEPARTMENT").OrderBy(s => s.Name)
             .Select(p => new SelectListItem()
             {
                 Text = p.Name,
                 Value = p.Id.ToString()
             }).ToList();

            ViewData["BuyDocSeriesListJs"] = buyDocSeriesListJs;
            ViewData["SellDocSeriesListJs"] = sellDocSeriesListJs;
            ViewData["SupplierListJs"] = supplierListJs;
            ViewData["CustomerListJs"] = customerListJs;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ItemVm).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyMaterialsDocumentExists(ItemVm.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index2");
        }

        private bool BuyMaterialsDocumentExists(int id)
        {
            return _context.BuyDocuments.Any(e => e.Id == id);
        }
    }
}
