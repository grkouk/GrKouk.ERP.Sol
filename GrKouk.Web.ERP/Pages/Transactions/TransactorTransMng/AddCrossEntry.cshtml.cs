using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class AddCrossEntry : PageModel
    {
        private const string _sectionCode = "SYS-TRANSACTOR-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        [BindProperty] public TransactorCrossEntryDto ItemVm { get; set; }

        public AddCrossEntry(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCombosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCombosAsync();
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

            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                #region transaction 1

                var docSeries1 = await
                    _context.TransTransactorDocSeriesDefs.SingleOrDefaultAsync(m =>
                        m.Id == ItemVm.DocSeries1Id);
                if (docSeries1 is null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    await LoadCombosAsync();
                    return Page();
                }

                await _context.Entry(docSeries1).Reference(t => t.TransTransactorDocTypeDef).LoadAsync();
                var docTypeDef1 = docSeries1.TransTransactorDocTypeDef;
                await _context.Entry(docTypeDef1)
                    .Reference(t => t.TransTransactorDef)
                    .LoadAsync();

                var transTransactorDef1 = docTypeDef1.TransTransactorDef;

                #region Section Management

                int sectionId1 = 0;
                if (docTypeDef1.SectionId == 0)
                {
                    var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _sectionCode);
                    if (sectn == null)
                    {
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        await LoadCombosAsync();
                        return Page();
                    }

                    sectionId1 = sectn.Id;
                }
                else
                {
                    sectionId1 = docTypeDef1.SectionId;
                }

                #endregion

                TransactorTransaction spTransaction1 = new TransactorTransaction
                {
                    TransDate = ItemVm.TransDate,
                    TransactorId = ItemVm.Transactor1Id,
                    CfAccountId = ItemVm.Cfa1Id,
                    SectionId = sectionId1,
                    TransTransactorDocSeriesId = docSeries1.Id,
                    TransTransactorDocTypeId = docSeries1.TransTransactorDocTypeDefId,
                    FiscalPeriodId = fiscalPeriod.Id,
                    CompanyId = ItemVm.CompanyId,
                    Etiology = ItemVm.Etiology,
                    AmountNet = ItemVm.Amount,
                    TransRefCode = ItemVm.RefCode,

                    FinancialAction = transTransactorDef1.FinancialTransAction
                };
                ActionHandlers.TransactorFinAction(transTransactorDef1.FinancialTransAction, spTransaction1);
                await _context.TransactorTransactions.AddAsync(spTransaction1);
                await _context.SaveChangesAsync();
                if (ItemVm.Cfa1Id > 0)
                {
                    var cfaSeriesId1 = docSeries1.DefaultCfaTransSeriesId;
                    if (cfaSeriesId1 > 0)
                    {
                        var cfaSeries1 = await _context.CashFlowDocSeriesDefs.FindAsync(cfaSeriesId1);
                        if (cfaSeries1 != null)
                        {
                            await _context.Entry(cfaSeries1)
                                .Reference(t => t.CashFlowDocTypeDefinition)
                                .LoadAsync();

                            var cfaType = cfaSeries1.CashFlowDocTypeDefinition;
                            if (cfaType != null)
                            {
                                await _context.Entry(cfaType)
                                    .Reference(t => t.CashFlowTransactionDefinition)
                                    .LoadAsync();
                                var transactor1 = await _context.Transactors
                                    .Where(p => p.Id == ItemVm.Transactor1Id)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync();
                                var etiology =
                                    $"{cfaSeries1.Name} created from {docSeries1.Name} for {transactor1.Name} with {ItemVm.Etiology} ";

                                var cfaTransDef1 = cfaType.CashFlowTransactionDefinition;
                                var cfaTrans1 = new CashFlowAccountTransaction
                                {
                                    TransDate = ItemVm.TransDate,
                                    CashFlowAccountId = ItemVm.Cfa1Id,
                                    CompanyId = ItemVm.CompanyId,
                                    DocumentSeriesId = cfaSeries1.Id,
                                    DocumentTypeId = cfaType.Id,
                                    Etiology = etiology,
                                    FiscalPeriodId = spTransaction1.FiscalPeriodId,
                                    CreatorSectionId = sectionId1,
                                    CreatorId = spTransaction1.Id,
                                    RefCode = spTransaction1.TransRefCode,
                                    Amount = ItemVm.Amount,
                                    SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId1
                                };
                                ActionHandlers.CashFlowFinAction(cfaTransDef1.CfaAction, cfaTrans1);
                                await _context.CashFlowAccountTransactions.AddAsync(cfaTrans1);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                #endregion

                #region transaction 2

                var docSeries2 = await
                    _context.TransTransactorDocSeriesDefs.SingleOrDefaultAsync(m =>
                        m.Id == ItemVm.DocSeries2Id);
                if (docSeries2 is null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    await LoadCombosAsync();
                    return Page();
                }

                await _context.Entry(docSeries2).Reference(t => t.TransTransactorDocTypeDef).LoadAsync();
                var docTypeDef2 = docSeries2.TransTransactorDocTypeDef;
                await _context.Entry(docTypeDef2)
                    .Reference(t => t.TransTransactorDef)
                    .LoadAsync();

                var transTransactorDef2 = docTypeDef2.TransTransactorDef;

                #region Section Management

                int sectionId2 = 0;
                if (docTypeDef2.SectionId == 0)
                {
                    var sectn = await _context.Sections.SingleOrDefaultAsync(s => s.SystemName == _sectionCode);
                    if (sectn == null)
                    {
                        ModelState.AddModelError(string.Empty, "Δεν υπάρχει το Section");
                        await LoadCombosAsync();
                        return Page();
                    }

                    sectionId2 = sectn.Id;
                }
                else
                {
                    sectionId2 = docTypeDef2.SectionId;
                }

                #endregion

                TransactorTransaction spTransaction2 = new TransactorTransaction
                {
                    TransDate = ItemVm.TransDate,
                    TransactorId = ItemVm.Transactor2Id,
                    CfAccountId = ItemVm.Cfa2Id,
                    SectionId = sectionId2,
                    TransTransactorDocSeriesId = docSeries2.Id,
                    TransTransactorDocTypeId = docSeries2.TransTransactorDocTypeDefId,
                    FiscalPeriodId = fiscalPeriod.Id,
                    CompanyId = ItemVm.CompanyId,
                    Etiology = ItemVm.Etiology,
                    AmountNet = ItemVm.Amount,
                    TransRefCode = ItemVm.RefCode,

                    FinancialAction = transTransactorDef2.FinancialTransAction
                };
                ActionHandlers.TransactorFinAction(transTransactorDef2.FinancialTransAction, spTransaction2);
                await _context.TransactorTransactions.AddAsync(spTransaction2);
                await _context.SaveChangesAsync();
                if (ItemVm.Cfa2Id > 0)
                {
                    var cfaSeriesId2 = docSeries2.DefaultCfaTransSeriesId;
                    if (cfaSeriesId2 > 0)
                    {
                        var cfaSeries2 = await _context.CashFlowDocSeriesDefs.FindAsync(cfaSeriesId2);
                        if (cfaSeries2 != null)
                        {
                            await _context.Entry(cfaSeries2)
                                .Reference(t => t.CashFlowDocTypeDefinition)
                                .LoadAsync();

                            var cfaType = cfaSeries2.CashFlowDocTypeDefinition;
                            if (cfaType != null)
                            {
                                await _context.Entry(cfaType)
                                    .Reference(t => t.CashFlowTransactionDefinition)
                                    .LoadAsync();
                                var transactor2 = await _context.Transactors
                                    .Where(p => p.Id == ItemVm.Transactor2Id)

                                    .SingleOrDefaultAsync();
                                var etiology =
                                    $"{cfaSeries2.Name} created from {docSeries2.Name} for {transactor2.Name} with {ItemVm.Etiology} ";

                                var cfaTransDef2 = cfaType.CashFlowTransactionDefinition;
                                var cfaTrans2 = new CashFlowAccountTransaction
                                {
                                    TransDate = ItemVm.TransDate,
                                    CashFlowAccountId = ItemVm.Cfa1Id,
                                    CompanyId = ItemVm.CompanyId,
                                    DocumentSeriesId = cfaSeries2.Id,
                                    DocumentTypeId = cfaType.Id,
                                    Etiology = etiology,
                                    FiscalPeriodId = spTransaction2.FiscalPeriodId,
                                    CreatorSectionId = sectionId2,
                                    CreatorId = spTransaction2.Id,
                                    RefCode = spTransaction2.TransRefCode,
                                    Amount = ItemVm.Amount,
                                    SectionId = cfaType.SectionId > 0 ? cfaType.SectionId : sectionId2
                                };
                                ActionHandlers.CashFlowFinAction(cfaTransDef2.CfaAction, cfaTrans2);
                                await _context.CashFlowAccountTransactions.AddAsync(cfaTrans2);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                #endregion

                try
                {


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    ModelState.AddModelError(string.Empty, msg);
                    await LoadCombosAsync();
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }



        private async Task LoadCombosAsync()
        {
            Func<Task<List<SelectListItem>>> transactorListFunc = async () =>
            {
                var itemList = await _context.Transactors
                    .Include(p => p.TransactorType)
                    .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                    .OrderBy(s => s.Name)
                    .AsNoTracking()
                    .Select(c => new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name}-{c.TransactorType.Code}"
                    })
                    .ToListAsync();
                return itemList;
            };
            Func<Task<List<SelectListItem>>> companiesListFunc = async () => await FiltersHelper.GetSolidCompaniesFilterListAsync(_context);
            Func<Task<List<SelectListItem>>> fiscalPeriodListFunc = async () => await FiltersHelper.GetFiscalPeriodsFilterListAsync(_context);
            Func<Task<List<SelectListItem>>> transactorTransDocSeriesListFunc = async () =>
            {
                var itemsList = await _context.TransTransactorDocSeriesDefs
                    .OrderBy(s => s.Name)
                    .AsNoTracking()
                    .Select(c => new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<TransactorSelectListItem>>> transactorsJsListFunc = async () =>
            {
                var itemsList = await _context.Transactors
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
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<TransactorDocTypeAllowedTransactorTypes>>> allowedTransactorTypesJsListFunc = async () =>
            {
                var itemsList = await _context.TransTransactorDocSeriesDefs
                    .Include(p => p.TransTransactorDocTypeDef)
                    .Select(p => new TransactorDocTypeAllowedTransactorTypes()
                    {
                        DocSeriesId = p.Id,
                        DocTypeId = p.TransTransactorDocTypeDefId,
                        DefaultCfaId = p.TransTransactorDocTypeDef.DefaultCfaId,
                        AllowedTypes = p.TransTransactorDocTypeDef.AllowedTransactorTypes
                    })
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<SelectListItem>>> cfAccountsSelectListFunc = async () => await SelectListHelpers.GetCfAccountsNoSelectionListAsync(_context);

            ViewData["CompanyId"] = await companiesListFunc();
            ViewData["FiscalPeriodId"] = await fiscalPeriodListFunc();
            ViewData["TransactorId"] = await transactorListFunc();
            ViewData["DocSeriesId"] = await transactorTransDocSeriesListFunc();
            ViewData["transactorsListJs"] = await transactorsJsListFunc();
            ViewData["docTypeAllowedTransactorTypesListJs"] = await allowedTransactorTypesJsListFunc();
            ViewData["CfAccountId"] = await cfAccountsSelectListFunc();



        }
    }
}