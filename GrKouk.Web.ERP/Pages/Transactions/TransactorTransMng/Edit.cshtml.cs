using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.TransactorTransMng
{
    [Authorize(Roles = "Admin,Operator")]
    public class EditModel : PageModel
    {
        private const string _sectionCode = "SYS-TRANSACTOR-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
                private readonly ITransactorTransactionService _transService;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public int EntityInTransactionId { get; set; }

        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification, ITransactorTransactionService transService)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _transService = transService;
        }

        [BindProperty] public TransactorTransModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionToModify = await _context.TransactorTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.TransTransactorDocSeries)
                .Include(t => t.TransTransactorDocType)
                .Include(t => t.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null)
            {
                return NotFound();
            }

            EntityInTransactionId = transactionToModify.TransactorId;
            var section = _context.Sections.SingleOrDefault(s => s.SystemName == _sectionCode);
            if (section is null)
            {
                _toastNotification.AddAlertToastMessage("Supplier Transactions section not found in DB");
                return BadRequest();
            }

            //If section is not our section the canot update disable input controls
            //NotUpdatable = transactionToModify.SectionId != section.Id;
            NotUpdatable = transactionToModify.CreatorId != 0;
            ItemVm = _mapper.Map<TransactorTransModifyDto>(transactionToModify);
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            try
            {
                var serviceResult = await _transService.ModifyTransactorTransaction(ItemVm);
                if (serviceResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Empty response from transactor service");
                    LoadCombos();
                    return Page();
                }

                if (!serviceResult.Success)
                {
                    var msg = string.IsNullOrWhiteSpace(serviceResult.ErrorMessage)
                        ? "Error from transactor service"
                        : serviceResult.ErrorMessage;
                    ModelState.AddModelError(string.Empty, msg);
                    LoadCombos();
                    return Page();
                }
                _toastNotification.AddSuccessToastMessage("Transaction saved");
            }
            catch (Exception e)
            {
                var msg = $"Error {e.Message} {e.InnerException?.Message}";
                _toastNotification.AddErrorToastMessage(msg);
                ModelState.AddModelError(string.Empty, msg);
                LoadCombos();
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private bool TransactorTransactionExists(int id)
        {
            return _context.TransactorTransactions.Any(e => e.Id == id);
        }

        private void LoadCombos()
        {
            var transactorsListDb = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(s => s.Name).AsNoTracking();
            List<SelectListItem> transactorsList = new List<SelectListItem>();
            List<UISelectTypeItem> transactorsListUi = new List<UISelectTypeItem>();
            foreach (var dbTransactor in transactorsListDb)
            {
                transactorsList.Add(new SelectListItem()
                {
                    Value = dbTransactor.Id.ToString(),
                    Text = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code
                });
                // transactorsListUi.Add(new()
                // {
                //     Value = dbTransactor.Id.ToString(),
                //     Text = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code,
                //     Title = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code,
                //     ValueInt = dbTransactor.Id
                // });
            }

            ViewData["CompanyId"] = FiltersHelper.GetSolidCompaniesFilterList(_context);
                //new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] =
                new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["TransTransactorDocSeriesId"] =
                new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Code");
            var transactorsListJs = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(p => p.Name)
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
                    DefaultCfaId = p.TransTransactorDocTypeDef.DefaultCfaId,
                    AllowedTypes = p.TransTransactorDocTypeDef.AllowedTransactorTypes
                })
                .AsNoTracking()
                .ToList();
            ViewData["docTypeAllowedTransactorTypesListJs"] = docTypeAllowedTransactorTypesListJs;
            ViewData["CfAccountId"] = SelectListHelpers.GetCfAccountsNoSelectionList(_context);
            ViewData["TransactorIdUi"] = transactorsListUi;
        }
    }
}