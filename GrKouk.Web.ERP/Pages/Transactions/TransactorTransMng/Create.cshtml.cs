using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.TransactorTransMng
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private const string _sectionCode = "SYS-TRANSACTOR-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public int CopyFromId { get; set; }

        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public IActionResult OnGet(int? copyFromId)
        {
            LoadCombos();
            return Page();
        }

        [BindProperty]
        public TransactorTransCreateDto ItemVm { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //_context.TransactorTransactions.Add(TransactorTransaction);

            #region Fiscal Period
            //if (ItemVm.FiscalPeriodId <= 0)
            //{
            //    ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
            //    LoadCombos();
            //    return Page();
            //}
            var fiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(p =>
                ItemVm.TransDate >= p.StartDate && ItemVm.TransDate <= p.EndDate);
            if (fiscalPeriod == null)
            {
               
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                return Page();
            }
            #endregion
            var spTransaction = _mapper.Map<TransactorTransaction>(ItemVm);

            var docSeries = await
                _context.TransTransactorDocSeriesDefs.SingleOrDefaultAsync(m =>
                    m.Id == ItemVm.TransTransactorDocSeriesId);

            if (docSeries is null)
            {
                ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                LoadCombos();
                return Page();
            }
            await _context.Entry(docSeries).Reference(t => t.TransTransactorDocTypeDef).LoadAsync();

            var docTypeDef = docSeries.TransTransactorDocTypeDef;
            await _context.Entry(docTypeDef)
                .Reference(t => t.TransTransactorDef)
                .LoadAsync();

            var transTransactorDef = docTypeDef.TransTransactorDef;

            #region Section Management
            int sectionId = 0;
            if (docTypeDef.SectionId == 0)
            {
                var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _sectionCode);
                if (sectn == null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                    LoadCombos();
                    return Page();
                }

                sectionId = sectn.Id;
            }
            else
            {
                sectionId = docTypeDef.SectionId;
            }
            #endregion


            spTransaction.SectionId = sectionId;
            spTransaction.TransTransactorDocTypeId = docSeries.TransTransactorDocTypeDefId;
            spTransaction.FiscalPeriodId = fiscalPeriod.Id;
            spTransaction.FinancialAction = transTransactorDef.FinancialTransAction;
            switch (transTransactorDef.FinancialTransAction)
            {
                case FinActionsEnum.FinActionsEnumNoChange:
                    spTransaction.TransDiscountAmount = 0;
                    spTransaction.TransFpaAmount = 0;
                    spTransaction.TransNetAmount = 0;
                    break;
                case FinActionsEnum.FinActionsEnumDebit:
                    spTransaction.TransDiscountAmount = spTransaction.AmountDiscount;
                    spTransaction.TransFpaAmount = spTransaction.AmountFpa;
                    spTransaction.TransNetAmount = spTransaction.AmountNet;
                    break;
                case FinActionsEnum.FinActionsEnumCredit:
                    spTransaction.TransDiscountAmount = spTransaction.AmountDiscount;
                    spTransaction.TransFpaAmount = spTransaction.AmountFpa;
                    spTransaction.TransNetAmount = spTransaction.AmountNet;
                    break;
                case FinActionsEnum.FinActionsEnumNegativeDebit:
                    spTransaction.TransNetAmount = spTransaction.AmountNet * -1;
                    spTransaction.TransFpaAmount = spTransaction.AmountFpa * -1;
                    spTransaction.TransDiscountAmount = spTransaction.AmountDiscount * -1;
                    break;
                case FinActionsEnum.FinActionsEnumNegativeCredit:
                    spTransaction.TransNetAmount = spTransaction.AmountNet * -1;
                    spTransaction.TransFpaAmount = spTransaction.AmountFpa * -1;
                    spTransaction.TransDiscountAmount = spTransaction.AmountDiscount * -1;
                    break;
                default:
                    break;
            }


            _context.TransactorTransactions.Add(spTransaction);
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Saved");



            return RedirectToPage("./Index");
        }
        private void LoadCombos()
        {
            var transactorsListDb = _context.Transactors
                .Include(p=>p.TransactorType)
                .Where(p=>p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(s => s.Name).AsNoTracking();
            List<SelectListItem> transactorsList = new List<SelectListItem>();
            
            foreach (var dbTransactor in transactorsListDb)
            {

                transactorsList.Add(new SelectListItem() { Value = dbTransactor.Id.ToString(), Text = dbTransactor.Name +"-"+ dbTransactor.TransactorType.Code });
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] = new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["TransTransactorDocSeriesId"] = new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Code");
        }
    }
}