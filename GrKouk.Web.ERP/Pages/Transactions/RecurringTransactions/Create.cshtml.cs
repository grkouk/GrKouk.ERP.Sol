using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.RecurringTransactions
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public string SeekType { get; set; }
        public int RoutedCompanyId { get; set; }
        public int RoutedSectionId { get; set; }
        public bool InitialLoad = true;
        public int CopyFromId { get; set; }
        public int CreateFromId { get; set; }
        public RecurringDocTypeEnum CreateFromType { get; set; }

        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync(int? companyFilter, int? section, int? copyFromId, int? createFromId, int? createFromType)
        {
            RoutedCompanyId = companyFilter ?? 0;
            RoutedSectionId = section ?? 0;
            CopyFromId = copyFromId ?? 0;
            CreateFromId = createFromId ?? 0;
            if (createFromType > 0)
            {
                CreateFromType = (RecurringDocTypeEnum)createFromType;
            }


            //if (CopyFromId > 0)
            //{
            //    var buyMatDoc = await _context.BuyDocuments
            //        .Include(b => b.Company)
            //        .Include(b => b.FiscalPeriod)
            //        .Include(b => b.BuyDocSeries)
            //        .Include(b => b.BuyDocType)
            //        .Include(b => b.Section)

            //        .Include(b => b.Transactor)
            //        .Include(b => b.BuyDocLines)
            //        .ThenInclude(m => m.WarehouseItem)
            //        .FirstOrDefaultAsync(m => m.Id == CopyFromId);
            //    if (buyMatDoc == null)
            //    {
            //        return NotFound();
            //    }
            //    //ItemVm = _mapper.Map<BuyDocCreateAjaxDto>(buyMatDoc);
            //    CopyFromItemVm = _mapper.Map<BuyDocModifyDto>(buyMatDoc);
            //    ItemVm = new BuyDocCreateAjaxDto
            //    {
            //        AmountDiscount = CopyFromItemVm.AmountDiscount,
            //        AmountFpa = CopyFromItemVm.AmountFpa,
            //        AmountNet = CopyFromItemVm.AmountNet,
            //        BuyDocSeriesId = CopyFromItemVm.BuyDocSeriesId,
            //        CompanyId = CopyFromItemVm.CompanyId,
            //        Etiology = CopyFromItemVm.Etiology,
            //        PaymentMethodId = CopyFromItemVm.PaymentMethodId,
            //        TransactorId = CopyFromItemVm.TransactorId
            //    };
            //}
            if (CreateFromId > 0)
            {
                //RecurringTransDocCreateAjaxDto vm;
                switch (CreateFromType)
                {
                    case RecurringDocTypeEnum.BuyType:

                        var buyMatDoc = await _context.BuyDocuments
                            .Include(b => b.Company)
                            .Include(b => b.FiscalPeriod)
                            .Include(b => b.BuyDocSeries)
                            .Include(b => b.BuyDocType)
                            .Include(b => b.Section)

                            .Include(b => b.Transactor)
                            .Include(b => b.BuyDocLines)
                            .ThenInclude(m => m.WarehouseItem)
                            .FirstOrDefaultAsync(m => m.Id == CreateFromId);
                        if (buyMatDoc == null)
                        {
                            return NotFound();
                        }
                        //ItemVm = _mapper.Map<BuyDocCreateAjaxDto>(buyMatDoc);
                        CopyFromItemVm = _mapper.Map<RecurringDocModifyDto>(buyMatDoc);
                        ItemVm = new RecurringTransDocCreateAjaxDto
                        {
                            NextTransDate = buyMatDoc.TransDate.AddMonths(1),
                            RecurringFrequency = "1M",
                            RecurringDocType = RecurringDocTypeEnum.BuyType,
                            SectionId = CopyFromItemVm.SectionId,
                            AmountDiscount = CopyFromItemVm.AmountDiscount,
                            AmountFpa = CopyFromItemVm.AmountFpa,
                            AmountNet = CopyFromItemVm.AmountNet,
                            DocSeriesId = CopyFromItemVm.DocSeriesId,
                            CompanyId = CopyFromItemVm.CompanyId,
                            Etiology = CopyFromItemVm.Etiology,
                            PaymentMethodId = CopyFromItemVm.PaymentMethodId,
                            TransactorId = CopyFromItemVm.TransactorId
                        };
                        //Check if there is another recurring transaction with similar data
                        var r = await _context.RecurringTransDocs.SingleOrDefaultAsync(p =>
                            p.TransactorId == CopyFromItemVm.TransactorId
                            && p.CompanyId == CopyFromItemVm.CompanyId
                            && p.RecurringDocType == RecurringDocTypeEnum.BuyType
                            && p.DocSeriesId == CopyFromItemVm.DocSeriesId);
                        if (r != null)
                        {
                            //There is another similar recurring transaction doc
                            ThereIsAnotherDoc = true;
                            ExistingRecTransDoc = new RecurringDocModifyDto
                            {
                                Id = r.Id,
                                RecurringFrequency = r.RecurringFrequency,
                                RecurringDocType = r.RecurringDocType,
                                NextTransDate = r.NextTransDate,

                                TransactorId = r.TransactorId,
                                TransactorName = r.Transactor.Name,
                                DocSeriesId = r.DocSeriesId,
                                DocSeriesCode = buyMatDoc.BuyDocSeries.Code,
                                DocSeriesName = buyMatDoc.BuyDocSeries.Name,
                                AmountFpa = r.AmountFpa,
                                AmountNet = r.AmountNet,
                                AmountDiscount = r.AmountDiscount,
                                CompanyId = r.CompanyId,
                                CompanyCode = r.Company.Code,
                                PaymentMethodId = r.PaymentMethodId
                            };
                        }
                        else
                        {
                            ThereIsAnotherDoc = false;
                            ExistingRecTransDoc = null;
                        }

                        break;
                    case RecurringDocTypeEnum.SellType:
                        var sellMatDoc = await _context.SellDocuments
                               .Include(b => b.Company)
                               .Include(b => b.FiscalPeriod)
                               .Include(b => b.SellDocSeries)
                               .Include(b => b.SellDocType)
                               .Include(b => b.Section)

                               .Include(b => b.Transactor)
                               .Include(b => b.SellDocLines)
                               .ThenInclude(m => m.WarehouseItem)
                               .FirstOrDefaultAsync(m => m.Id == CreateFromId);
                        if (sellMatDoc == null)
                        {
                            return NotFound();
                        }
                        //ItemVm = _mapper.Map<BuyDocCreateAjaxDto>(buyMatDoc);
                        // _mapper.ConfigurationProvider.AssertConfigurationIsValid();
                        CopyFromItemVm = _mapper.Map<RecurringDocModifyDto>(sellMatDoc);
                        ItemVm = new RecurringTransDocCreateAjaxDto
                        {
                            NextTransDate = sellMatDoc.TransDate.AddMonths(1),
                            RecurringFrequency = "1M",
                            RecurringDocType = RecurringDocTypeEnum.SellType,
                            SectionId = CopyFromItemVm.SectionId,
                            AmountDiscount = CopyFromItemVm.AmountDiscount,
                            AmountFpa = CopyFromItemVm.AmountFpa,
                            AmountNet = CopyFromItemVm.AmountNet,
                            DocSeriesId = CopyFromItemVm.DocSeriesId,
                            CompanyId = CopyFromItemVm.CompanyId,
                            Etiology = CopyFromItemVm.Etiology,
                            PaymentMethodId = CopyFromItemVm.PaymentMethodId,
                            TransactorId = CopyFromItemVm.TransactorId
                        };
                        //Check if there is another recurring transaction with similar data
                        var rSell = await _context.RecurringTransDocs.SingleOrDefaultAsync(p =>
                            p.TransactorId == CopyFromItemVm.TransactorId
                            && p.CompanyId == CopyFromItemVm.CompanyId
                            && p.RecurringDocType == RecurringDocTypeEnum.SellType
                            && p.DocSeriesId == CopyFromItemVm.DocSeriesId);
                        if (rSell != null)
                        {
                            //There is another similar recurring transaction doc
                            ThereIsAnotherDoc = true;
                            ExistingRecTransDoc = new RecurringDocModifyDto
                            {
                                Id = rSell.Id,
                                RecurringFrequency = rSell.RecurringFrequency,
                                RecurringDocType = rSell.RecurringDocType,
                                NextTransDate = rSell.NextTransDate,

                                TransactorId = rSell.TransactorId,
                                TransactorName = rSell.Transactor.Name,
                                DocSeriesId = rSell.DocSeriesId,
                                DocSeriesCode = sellMatDoc.SellDocSeries.Code,
                                DocSeriesName = sellMatDoc.SellDocSeries.Name,
                                AmountFpa = rSell.AmountFpa,
                                AmountNet = rSell.AmountNet,
                                AmountDiscount = rSell.AmountDiscount,
                                CompanyId = rSell.CompanyId,
                                CompanyCode = rSell.Company.Code,
                                PaymentMethodId = rSell.PaymentMethodId
                            };
                        }
                        else
                        {
                            ThereIsAnotherDoc = false;
                            ExistingRecTransDoc = null;
                        }
                        break;
                    default:
                        break;
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
            if (CreateFromType == RecurringDocTypeEnum.BuyType)
            {
                transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.SUPPLIER").OrderBy(s => s.Name).AsNoTracking();
                docTypeList = _context.BuyDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            }
            else
            {
                transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.CUSTOMER" || s.TransactorType.Code == "SYS.DEPARTMENT").OrderBy(s => s.Name).AsNoTracking();
                docTypeList = _context.SellDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            }

            ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");
            ViewData["DocSeriesId"] = new SelectList(docTypeList, "Id", "Name");

        }

        [BindProperty]
        public RecurringTransDocCreateAjaxDto ItemVm { get; set; }
        public RecurringDocModifyDto CopyFromItemVm { get; set; }
        public RecurringDocModifyDto ExistingRecTransDoc { get; private set; }
        public bool ThereIsAnotherDoc { get; set; }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }



            return RedirectToPage("./Index");
        }

    }
}