using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public string SeekType { get; set; }
        public bool InitialLoad = true;
        public int RoutedCompanyId { get; set; }
        public int RoutedSectionId { get; set; }
        public int CopyFromId { get; set; }

        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync(int? companyFilter, int? section, int? copyFromId)
        {
            RoutedCompanyId = companyFilter ?? 0;
            RoutedSectionId = section ?? 0;
            CopyFromId = copyFromId ?? 0;
            if (CopyFromId > 0)
            {
                var sellMatDoc = await _context.SellDocuments
                    .Include(b => b.Company)
                    .Include(b => b.FiscalPeriod)
                    .Include(b => b.SellDocSeries)
                    .Include(b => b.SellDocType)
                    .Include(b => b.Section)

                    .Include(b => b.Transactor)
                    .Include(b => b.SellDocLines)
                    .ThenInclude(m => m.WarehouseItem)
                    .FirstOrDefaultAsync(m => m.Id == CopyFromId);

                if (sellMatDoc == null)
                {
                    return NotFound();
                }
                //ItemVm = _mapper.Map<BuyDocCreateAjaxDto>(buyMatDoc);
                CopyFromItemVm = _mapper.Map<SellDocModifyDto>(sellMatDoc);
                if (CopyFromItemVm != null)
                {
                    ItemVm = new SellDocCreateAjaxDto()
                    {
                        AmountDiscount = CopyFromItemVm.AmountDiscount,
                        AmountFpa = CopyFromItemVm.AmountFpa,
                        AmountNet = CopyFromItemVm.AmountNet,
                        SellDocSeriesId = CopyFromItemVm.SellDocSeriesId,
                        CompanyId = CopyFromItemVm.CompanyId,
                        Etiology = CopyFromItemVm.Etiology,
                        PaymentMethodId = CopyFromItemVm.PaymentMethodId,
                        SalesChannelId = CopyFromItemVm.SalesChannelId,
                        TransactorId = CopyFromItemVm.TransactorId
                    };
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
            var transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.CUSTOMER" || s.TransactorType.Code == "SYS.DEPARTMENT").OrderBy(s => s.Name).AsNoTracking();
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["SellDocSeriesId"] = new SelectList(_context.SellDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SalesChannelId"] = new SelectList(_context.SalesChannels.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");

        }

        [BindProperty]
        public SellDocCreateAjaxDto ItemVm { get; set; }
        public SellDocModifyDto CopyFromItemVm { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //var itemToAttach = _mapper.Map<BuyDocument>(ItemVm);
            //_context.BuyDocuments.Add(itemToAttach);
            //await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

    }
}