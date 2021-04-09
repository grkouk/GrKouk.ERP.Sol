using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.TransactorTransMng {
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel {
        private const string _sectionCode = "SYS-TRANSACTOR-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public int EntityInTransactionId { get; set; }
        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification) {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public TransactorTransModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id) {
            if (id == null) {
                return NotFound();
            }

            var transactionToModify = await _context.TransactorTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.TransTransactorDocSeries)
                .Include(t => t.TransTransactorDocType)
                .Include(t => t.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null) {
                return NotFound();
            }

            EntityInTransactionId = transactionToModify.TransactorId;
            var section = _context.Sections.SingleOrDefault(s => s.SystemName == _sectionCode);
            if (section is null) {
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

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                LoadCombos();
                return Page();
            }
            var spTransactionToAttach = _mapper.Map<TransactorTransaction>(ItemVm);

            #region Fiscal Period
            var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, ItemVm.TransDate);
            if (fiscalPeriod == null) {
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                return Page();
            }
            #endregion
            var docSeries = _context.TransTransactorDocSeriesDefs.SingleOrDefault(m => m.Id == spTransactionToAttach.TransTransactorDocSeriesId);

            if (docSeries is null) {
                ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                LoadCombos();
                return Page();
            }
            _context.Entry(docSeries).Reference(t => t.TransTransactorDocTypeDef).Load();

            var docTypeDef = docSeries.TransTransactorDocTypeDef;
            _context.Entry(docTypeDef)
                .Reference(t => t.TransTransactorDef)
                .Load();
            var transTransactorDef = docTypeDef.TransTransactorDef;
            #region Section Management
            int sectionId = 0;
            if (docTypeDef.SectionId == 0) {
                var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _sectionCode);
                if (sectn == null) {
                    ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                    LoadCombos();
                    return Page();
                }

                sectionId = sectn.Id;
            }
            else {
                sectionId = docTypeDef.SectionId;
            }
            #endregion
            var spOldTrans = await _context.TransactorTransactions
                .Where(p=>p.Id==ItemVm.Id)
                .AsNoTracking()
                .SingleOrDefaultAsync();
            int oldTransSectionId = spOldTrans?.SectionId ?? sectionId;
            await using (var transaction = await _context.Database.BeginTransactionAsync()) {
                spTransactionToAttach.SectionId = sectionId;
                spTransactionToAttach.FiscalPeriodId = fiscalPeriod.Id;
                spTransactionToAttach.TransTransactorDocTypeId = docSeries.TransTransactorDocTypeDefId;
                spTransactionToAttach.FinancialAction = transTransactorDef.FinancialTransAction;
                ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, spTransactionToAttach);
                try {
                    _context.Attach(spTransactionToAttach).State = EntityState.Modified;
                    var docId = spTransactionToAttach.Id;
                    _context.BuyDocTransPaymentMappings.RemoveRange(
                        _context.BuyDocTransPaymentMappings
                            .Where(p => p.TransactorTransactionId == docId));
                    _context.SellDocTransPaymentMappings.RemoveRange(
                        _context.SellDocTransPaymentMappings
                            .Where(p => p.TransactorTransactionId == docId));
                    _context.CashFlowAccountTransactions.RemoveRange(
                        _context.CashFlowAccountTransactions.Where(p =>
                            p.CreatorSectionId == oldTransSectionId && p.CreatorId == docId));
                    if (ItemVm.CfAccountId > 0) {
                        var cfaSeriesId = docSeries.DefaultCfaTransSeriesId;
                        if (cfaSeriesId > 0) {
                            var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(cfaSeriesId);
                            if (cfaSeries != null) {
                                await _context.Entry(cfaSeries)
                                    .Reference(t => t.CashFlowDocTypeDefinition)
                                    .LoadAsync();

                                var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                                if (cfaType != null) {
                                    await _context.Entry(cfaType)
                                        .Reference(t => t.CashFlowTransactionDefinition)
                                        .LoadAsync();
                                    var transactor = await _context.Transactors
                                        .Where(p => p.Id == ItemVm.TransactorId)
                                        .AsNoTracking()
                                        .SingleOrDefaultAsync();
                                    var etiology =
                                        $"{cfaSeries.Name} created from {docSeries.Name} for {transactor.Name} with {ItemVm.Etiology} ";
                                    var cfaTransDef = cfaType.CashFlowTransactionDefinition;
                                    var cfaTrans = new CashFlowAccountTransaction {
                                        TransDate = ItemVm.TransDate,
                                        CashFlowAccountId = ItemVm.CfAccountId,
                                        CompanyId = ItemVm.CompanyId,
                                        DocumentSeriesId = cfaSeries.Id,
                                        DocumentTypeId = cfaType.Id,
                                        Etiology = ItemVm.Etiology,
                                        FiscalPeriodId = spTransactionToAttach.FiscalPeriodId,
                                        CreatorSectionId = sectionId,
                                        CreatorId = spTransactionToAttach.Id,
                                        RefCode = spTransactionToAttach.TransRefCode,
                                        Amount = ItemVm.AmountSum,
                                        SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                    };
                                    ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                    await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                   
                                }
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Concurrency error");
                    LoadCombos();
                    return Page();
                }
                catch (Exception ex) {
                    await transaction.RollbackAsync();
                    string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
                    ModelState.AddModelError(string.Empty, msg);
                    LoadCombos();
                    return Page();
                }
            }


            return RedirectToPage("./Index");


        }

        private bool TransactorTransactionExists(int id) {
            return _context.TransactorTransactions.Any(e => e.Id == id);
        }
        private void LoadCombos() {
            var transactorsListDb = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(s => s.Name).AsNoTracking();
            List<SelectListItem> transactorsList = new List<SelectListItem>();

            foreach (var dbTransactor in transactorsListDb) {

                transactorsList.Add(new SelectListItem() { Value = dbTransactor.Id.ToString(), Text = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code });
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] = new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["TransTransactorDocSeriesId"] = new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Code");
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
                    DefaultCfaId = p.TransTransactorDocTypeDef.DefaultCfaId,
                    AllowedTypes = p.TransTransactorDocTypeDef.AllowedTransactorTypes
                })
                .AsNoTracking()
                .ToList();
            ViewData["docTypeAllowedTransactorTypesListJs"] = docTypeAllowedTransactorTypesListJs;
            ViewData["CfAccountId"] = SelectListHelpers.GetCfAccountsNoSelectionList(_context);
        }
    }
}
