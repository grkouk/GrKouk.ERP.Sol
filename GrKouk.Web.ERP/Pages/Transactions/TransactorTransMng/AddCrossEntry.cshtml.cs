using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
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
        [BindProperty] public CrossEntryDto ItemVm { get; set; }

        public AddCrossEntry(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public void OnGet()
        {
            LoadCombos();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
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
                    LoadCombos();
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
                        LoadCombos();
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

                #endregion

                #region transaction 2

                var docSeries2 = await
                    _context.TransTransactorDocSeriesDefs.SingleOrDefaultAsync(m =>
                        m.Id == ItemVm.DocSeries2Id);
                if (docSeries2 is null)
                {
                    ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                    LoadCombos();
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
                        LoadCombos();
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

                #endregion

                try
                {
                    await _context.TransactorTransactions.AddAsync(spTransaction1);
                    await _context.TransactorTransactions.AddAsync(spTransaction2);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                   await transaction.RollbackAsync();
                    string msg = e.InnerException?.Message;
                    ModelState.AddModelError(string.Empty, msg);
                    LoadCombos();
                    return Page();
                }
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

            ViewData["CompanyId"] = FiltersHelper.GetSolidCompaniesFilterList(_context);
                //new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] =
                new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["DocSeriesId"] =
                new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
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

    public class CrossEntryDto
    {
        [DataType(DataType.Date)] 
        [Display(Name = "Ημερομηνία")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Παραστατικο΄1")]
        public int DocSeries1Id { get; set; }
        [Display(Name = "Παραστατικό 2")]
        public int DocSeries2Id { get; set; }
        [Display(Name = "Συναλλασσόμενος 1")]
        public int Transactor1Id { get; set; }
        [Display(Name = "Συναλλασσόμενος 2")]
        public int Transactor2Id { get; set; }
        [Display(Name = "Χρημ.Λογ.1")]
        public int Cfa1Id { get; set; }
        [Display(Name = "Χρημ.Λογ 2")]
        public int Cfa2Id { get; set; }
        [Display(Name = "Αρ.Παραστατικού")]
        public string RefCode { get; set; }
        [Display(Name = "Αιτιολογία")]
        public string Etiology { get; set; }
        
        public decimal Amount { get; set; }
        [Display(Name = "Εταιρεία")]
        public int CompanyId { get; set; }
    }
}