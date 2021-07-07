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

                var transactionToModify = _context.TransactorTransactions
                    .Include(t => t.Company)
                    .Include(t => t.FiscalPeriod)
                    .Include(t => t.Section)
                    .Include(t => t.TransTransactorDocSeries)
                    .Include(t => t.TransTransactorDocType)
                    .Include(t => t.Transactor).FirstOrDefault(m => m.Id == CopyFromId);

                if (transactionToModify == null)
                {
                    return NotFound();
                }

                ItemVm = _mapper.Map<TransactorTransCreateDto>(transactionToModify);
                CopyFromTransactorId = ItemVm.TransactorId;
            }

            return Page();
        }

        [BindProperty] public TransactorTransCreateDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //_context.TransactorTransactions.Add(TransactorTransaction);

            #region Fiscal Period

            var fiscalPeriod = await HelperFunctions.GetFiscalPeriod(_context, ItemVm.TransDate);
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

            int sectionId ;
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

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                spTransaction.SectionId = sectionId;
                spTransaction.TransTransactorDocTypeId = docSeries.TransTransactorDocTypeDefId;
                spTransaction.FiscalPeriodId = fiscalPeriod.Id;
                spTransaction.FinancialAction = transTransactorDef.FinancialTransAction;
                ActionHandlers.TransactorFinAction(transTransactorDef.FinancialTransAction, spTransaction);
          
                await _context.TransactorTransactions.AddAsync(spTransaction);
                await _context.SaveChangesAsync();
                if (ItemVm.CfAccountId>0)
                {
                    var cfaSeriesId = docSeries.DefaultCfaTransSeriesId;
                    if (cfaSeriesId>0)
                    {
                        var cfaSeries = await _context.CashFlowDocSeriesDefs.FindAsync(cfaSeriesId);
                        if (cfaSeries!=null)
                        {
                            await _context.Entry(cfaSeries)
                                .Reference(t => t.CashFlowDocTypeDefinition)
                                .LoadAsync();

                            var cfaType = cfaSeries.CashFlowDocTypeDefinition;
                            if (cfaType!=null)
                            {
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
                                    Etiology = etiology,
                                    FiscalPeriodId = spTransaction.FiscalPeriodId,
                                    CreatorSectionId = sectionId,
                                    CreatorId = spTransaction.Id,
                                    RefCode = spTransaction.TransRefCode,
                                    Amount = ItemVm.AmountSum,
                                    SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId
                                };
                                ActionHandlers.CashFlowFinAction(cfaTransDef.CfaAction, cfaTrans);
                                await _context.CashFlowAccountTransactions.AddAsync(cfaTrans);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                
                await transaction.CommitAsync();
                _toastNotification.AddSuccessToastMessage("Saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                string msg = e.Message;
                msg += e.InnerException?.Message;

                await transaction.RollbackAsync();
                _toastNotification.AddErrorToastMessage(msg);
            }


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

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] =
                new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["TransTransactorDocSeriesId"] =
                new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            var transactorsListJs = _context.Transactors
                .Include(p => p.TransactorType)
                .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                .OrderBy(p=>p.Name)
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