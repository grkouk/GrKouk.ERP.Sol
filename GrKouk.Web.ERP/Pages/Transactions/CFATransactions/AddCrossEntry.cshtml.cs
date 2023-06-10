using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
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
    public class AddCrossEntry : PageModel
    {
        private const string _sectionCode = "SYS-CFA-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        [BindProperty] public CashFlowAccountCrossEntryDto ItemVm { get; set; }

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
                    _context.CashFlowDocSeriesDefs.SingleOrDefaultAsync(m =>
                        m.Id == ItemVm.DocSeries1Id);
                if (docSeries1 is null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    await LoadCombosAsync();
                    return Page();
                }

                await _context.Entry(docSeries1).Reference(t => t.CashFlowDocTypeDefinition).LoadAsync();
                var docTypeDef1 = docSeries1.CashFlowDocTypeDefinition;
                await _context.Entry(docTypeDef1)
                    .Reference(t => t.CashFlowTransactionDefinition)
                    .LoadAsync();

                var cfaTransactorDef1 = docTypeDef1.CashFlowTransactionDefinition;

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

                CashFlowAccountTransaction spTransaction1 = new()
                {
                    TransDate = ItemVm.TransDate,

                    CashFlowAccountId = ItemVm.Cfa1Id,
                    SectionId = sectionId1,
                    CreatorSectionId = sectionId1,
                    DocumentSeriesId = docSeries1.Id,
                    DocumentTypeId = docSeries1.CashFlowDocTypeDefId,
                    FiscalPeriodId = fiscalPeriod.Id,
                    CompanyId = ItemVm.CompanyId,
                    Etiology = ItemVm.Etiology,
                    Amount = ItemVm.Amount,
                    RefCode = ItemVm.RefCode,
                    CfaAction = cfaTransactorDef1.CfaAction
                };
                ActionHandlers.CashFlowFinAction(cfaTransactorDef1.CfaAction, spTransaction1);
                await _context.CashFlowAccountTransactions.AddAsync(spTransaction1);
                //await _context.SaveChangesAsync();

                #endregion

                #region transaction 2

                var docSeries2 = await
                    _context.CashFlowDocSeriesDefs.SingleOrDefaultAsync(m =>
                        m.Id == ItemVm.DocSeries2Id);
                if (docSeries2 is null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    await LoadCombosAsync();
                    return Page();
                }

                await _context.Entry(docSeries2).Reference(t => t.CashFlowDocTypeDefinition).LoadAsync();
                var docTypeDef2 = docSeries2.CashFlowDocTypeDefinition;
                await _context.Entry(docTypeDef2)
                    .Reference(t => t.CashFlowTransactionDefinition)
                    .LoadAsync();

                var cfaTransactorDef2 = docTypeDef2.CashFlowTransactionDefinition;

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

                CashFlowAccountTransaction spTransaction2 = new()
                {
                    TransDate = ItemVm.TransDate,
                    CashFlowAccountId = ItemVm.Cfa2Id,
                    SectionId = sectionId2,
                    CreatorSectionId = sectionId2,
                    DocumentSeriesId = docSeries2.Id,
                    DocumentTypeId = docSeries2.CashFlowDocTypeDefId,
                    FiscalPeriodId = fiscalPeriod.Id,
                    CompanyId = ItemVm.CompanyId,
                    Etiology = ItemVm.Etiology,
                    Amount = ItemVm.Amount,
                    RefCode = ItemVm.RefCode,
                    CfaAction = cfaTransactorDef2.CfaAction

                };
                ActionHandlers.CashFlowFinAction(cfaTransactorDef2.CfaAction, spTransaction2);
                await _context.CashFlowAccountTransactions.AddAsync(spTransaction2);
                //await _context.SaveChangesAsync();

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
            Func<Task<List<SelectListItem>>> cfaTransSeriesListFunc = async () =>
            {
                var itemsList = await _context.CashFlowDocSeriesDefs
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
            Func<Task<List<CashFlowAccountSelectListItem>>> cashFlowAccountsJsListFunc = async () =>
            {
                var itemsList = await _context.CashFlowAccounts
                        .OrderBy(p => p.Name)
                        .Select(p => new CashFlowAccountSelectListItem()
                        {
                            Id = p.Id,

                            CfaName = p.Name,
                            Value = p.Id.ToString(),
                            Text = p.Name
                        })
                        .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };

            Func<Task<List<AllowedCompanyCashFlowAccountItem>>> allowedCompanyCashFlowAccountsJsListFunc = async () =>
            {
                var itemsList = await _context.CashFlowAccountCompanyMappings
                    .Select(p => new AllowedCompanyCashFlowAccountItem()
                    {
                        CfaId = p.CashFlowAccountId,
                        CompanyId = p.CompanyId
                    })
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<SelectListItem>>> cfAccountsSelectListFunc = async () => await SelectListHelpers.GetCfAccountsNoSelectionListAsync(_context);

            ViewData["CompanyId"] = await companiesListFunc();
            ViewData["FiscalPeriodId"] = await fiscalPeriodListFunc();
            //ViewData["TransactorId"] = await transactorListFunc();
            ViewData["DocSeriesId"] = await cfaTransSeriesListFunc();
            //ViewData["transactorsListJs"] = await transactorsJsListFunc();
            //ViewData["cfaListJs"] = await cashFlowAccountsJsListFunc();
            // ViewData["docTypeAllowedTransactorTypesListJs"] = await allowedTransactorTypesJsListFunc();
            //ViewData["CompanyAllowedCashFloeAccounts"] = await allowedCompanyCashFlowAccountsJsListFunc();
            //ViewData["CfAccountId"] = await cfAccountsSelectListFunc();

        }
    }
}