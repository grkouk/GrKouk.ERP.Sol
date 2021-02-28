using System;
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
    public class EditModel : PageModel
    {
        private const string _sectionCode = "SYS-CFA-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public CfaTransactionModifyDto ItemVm { get; set; }

        public int CfaId { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionToModify = await _context.CashFlowAccountTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.DocumentSeries)
                .Include(t => t.DocumentType)
                .Include(t => t.CashFlowAccount)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null)
            {
                return NotFound();
            }

            CfaId = transactionToModify.CashFlowAccountId;
            var section = _context.Sections.SingleOrDefault(s => s.SystemName == _sectionCode);
            if (section is null)
            {
                _toastNotification.AddAlertToastMessage("Supplier Transactions section not found in DB");
                return BadRequest();
            }
            //If section is not our section the canot update disable input controls
            //NotUpdatable = transactionToModify.SectionId != section.Id;
            NotUpdatable=transactionToModify.CreatorId!=0;
            ItemVm = _mapper.Map<CfaTransactionModifyDto>(transactionToModify);
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
            if (ItemVm.FiscalPeriodId <= 0)
            {
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                LoadCombos();
                return Page();
            }

            var spTransactionToAttach = _mapper.Map<CashFlowAccountTransaction>(ItemVm);
            #region Fiscal Period
            var dateOfTrans = ItemVm.TransDate;
            var fiscalPeriod = await _context.FiscalPeriods.FirstOrDefaultAsync(p =>
                p.StartDate.CompareTo(dateOfTrans) > 0 & p.EndDate.CompareTo(dateOfTrans) < 0);
            if (fiscalPeriod == null) {
                ModelState.AddModelError(string.Empty, "No Fiscal Period covers Transaction Date");
                LoadCombos();
                return Page();
            }
            #endregion
            var docSeries = _context.CashFlowDocSeriesDefs.SingleOrDefault(m => m.Id == spTransactionToAttach.DocumentSeriesId);

            if (docSeries is null)
            {
                ModelState.AddModelError(string.Empty, "Δεν βρέθηκε η σειρά παραστατικού");
                LoadCombos();
                return Page();
            }
            _context.Entry(docSeries).Reference(t => t.CashFlowDocTypeDefinition).Load();

            var docTypeDef = docSeries.CashFlowDocTypeDefinition;
            _context.Entry(docTypeDef)
                .Reference(t => t.CashFlowTransactionDefinition)
                .Load();
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

            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                spTransactionToAttach.SectionId = sectionId;
                spTransactionToAttach.DocumentTypeId = docSeries.CashFlowDocTypeDefId;
                spTransactionToAttach.FiscalPeriodId = fiscalPeriod.Id;
                spTransactionToAttach.CfaAction = cfaTransactionDef.CfaAction;
                ActionHandlers.CashFlowFinAction(cfaTransactionDef.CfaAction, spTransactionToAttach);
                try
                {
                    _context.Attach(spTransactionToAttach).State = EntityState.Modified;
                    var docId = spTransactionToAttach.Id;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Concurrency error");
                    LoadCombos();
                    return Page();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
                    ModelState.AddModelError(string.Empty,msg );
                    LoadCombos();
                    return Page();
                }
            }
            return RedirectToPage("./Index");
        }

        private bool TransactionExists(int id)
        {
            return _context.CashFlowAccountTransactions.Any(e => e.Id == id);
        }
        private void LoadCombos()
        {
            var companiesList = FiltersHelper.GetSolidCompaniesFilterList(_context);
            ViewData["CompanyId"] = new SelectList(companiesList, "Value", "Text");
           
            ViewData["DocSeriesId"] =
                new SelectList(_context.CashFlowDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
        }
    }
}
