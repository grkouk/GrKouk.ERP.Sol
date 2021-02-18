using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.CFATransactions
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private const string _sectionCode = "SYS-CFA-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public int CopyFromId { get; set; }
        public int CopyFromTransactorId { get; set; } = 0;

        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public IActionResult OnGet(int? copyFromId)
        {
            LoadCombos();
            CopyFromId = 0;
            if (copyFromId != null)
            {
                CopyFromId = (int) copyFromId;

                var transactionToModify = _context.CashFlowAccountTransactions
                    .Include(t => t.Company)
                    .Include(t => t.FiscalPeriod)
                    .Include(t => t.Section)
                    .Include(t => t.DocumentSeries)
                    .Include(t => t.DocumentType)
                    .Include(t => t.CashFlowAccount)
                    .FirstOrDefault(m => m.Id == CopyFromId);

                if (transactionToModify == null)
                {
                    return NotFound();
                }

                ItemVm = _mapper.Map<CfaTransactionCreateDto>(transactionToModify);
                CopyFromTransactorId = ItemVm.CashFlowAccountId;
            }

            return Page();
        }

        [BindProperty] public CfaTransactionCreateDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            #region Fiscal Period

            var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, ItemVm.TransDate);
            if (fiscalPeriod == null)
            {
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                return Page();
            }

            #endregion

            var cfaTransaction = _mapper.Map<CashFlowAccountTransaction>(ItemVm);

            var docSeries = await
                _context.CashFlowDocSeriesDefs.SingleOrDefaultAsync(m =>
                    m.Id == ItemVm.DocSeriesId);

            if (docSeries is null)
            {
                ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                LoadCombos();
                return Page();
            }

            await _context.Entry(docSeries).Reference(t => t.CashFlowDocTypeDefinition).LoadAsync();

            var docTypeDef = docSeries.CashFlowDocTypeDefinition;
            await _context.Entry(docTypeDef)
                .Reference(t => t.CashFlowTransactionDefinition)
                .LoadAsync();

            var cfaTransactionDef = docTypeDef.CashFlowTransactionDefinition;

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


            cfaTransaction.SectionId = sectionId;
            cfaTransaction.DocumentTypeId = docSeries.CashFlowDocTypeDefId;
            cfaTransaction.FiscalPeriodId = fiscalPeriod.Id;
            cfaTransaction.CfaAction = cfaTransactionDef.CfaAction;
            ActionHandlers.CashFlowFinAction(cfaTransactionDef.CfaAction, cfaTransaction);


            //switch (cfaTransactionDef.FinancialTransAction)
            //{
            //    case FinActionsEnum.FinActionsEnumNoChange:
            //        cfaTransaction.TransDiscountAmount = 0;
            //        cfaTransaction.TransFpaAmount = 0;
            //        cfaTransaction.TransNetAmount = 0;
            //        break;
            //    case FinActionsEnum.FinActionsEnumDebit:
            //        cfaTransaction.TransDiscountAmount = cfaTransaction.AmountDiscount;
            //        cfaTransaction.TransFpaAmount = cfaTransaction.AmountFpa;
            //        cfaTransaction.TransNetAmount = cfaTransaction.AmountNet;
            //        break;
            //    case FinActionsEnum.FinActionsEnumCredit:
            //        cfaTransaction.TransDiscountAmount = cfaTransaction.AmountDiscount;
            //        cfaTransaction.TransFpaAmount = cfaTransaction.AmountFpa;
            //        cfaTransaction.TransNetAmount = cfaTransaction.AmountNet;
            //        break;
            //    case FinActionsEnum.FinActionsEnumNegativeDebit:
            //        cfaTransaction.TransNetAmount = cfaTransaction.AmountNet * -1;
            //        cfaTransaction.TransFpaAmount = cfaTransaction.AmountFpa * -1;
            //        cfaTransaction.TransDiscountAmount = cfaTransaction.AmountDiscount * -1;
            //        break;
            //    case FinActionsEnum.FinActionsEnumNegativeCredit:
            //        cfaTransaction.TransNetAmount = cfaTransaction.AmountNet * -1;
            //        cfaTransaction.TransFpaAmount = cfaTransaction.AmountFpa * -1;
            //        cfaTransaction.TransDiscountAmount = cfaTransaction.AmountDiscount * -1;
            //        break;
            //    default:
            //        break;
            //}


            await _context.CashFlowAccountTransactions.AddAsync(cfaTransaction);
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Saved");


            return RedirectToPage("./Index");
        }

        private void LoadCombos()
        {
            var transactorsListDb = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(s => s.Name).AsNoTracking();
            List<SelectListItem> transactorsList = new List<SelectListItem>();

            foreach (var dbTransactor in transactorsListDb)
            {
                transactorsList.Add(new SelectListItem()
                {
                    Value = dbTransactor.Id.ToString(),
                    Text = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code
                });
            }
            ViewData["CashFlowAccountId"] = new SelectList(transactorsList, "Value", "Text");

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] =
                new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
           
            ViewData["DocSeriesId"] =
                new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            
            var transactorsListJs = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .Select(p => new TransactorSelectListItem()
                {
                    Id = p.Id,
                    TransactorName = p.Name,
                    TransactorTypeId = p.TransactorType.Id,
                    TransactorTypeCode = p.TransactorType.Code,
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} {{{p.TransactorType.Code}}}"
                })
                .AsNoTracking()
                .ToList();
            ViewData["transactorsListJs"] = transactorsListJs;
            var docTypeAllowedTransactorTypesListJs = _context.TransTransactorDocSeriesDefs
                .Include(p => p.TransTransactorDocTypeDef)
                .Select(p => new TransactorDocTypeAllowedTransactorTypes()
                {
                    DocSeriesId = p.Id,
                    DocTypeId = p.TransTransactorDocTypeDefId,
                    AllowedTypes = p.TransTransactorDocTypeDef.AllowedTransactorTypes
                })
                .AsNoTracking()
                .ToList();
            ViewData["docTypeAllowedTransactorTypesListJs"] = docTypeAllowedTransactorTypesListJs;
        }
    }
}