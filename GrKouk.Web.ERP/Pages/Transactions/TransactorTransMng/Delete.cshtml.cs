using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.TransactorTransMng {
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITransactorTransactionService _transService;
        public bool NotUpdatable;
        public DeleteModel(ApiDbContext context, IMapper mapper, ITransactorTransactionService transService) {
            _context = context;
            _mapper = mapper;
            _transService = transService;
        }

        [BindProperty]
        public TransactorTransModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id) {
            if (id == null) {
                return BadRequest();
            }

            var transactionToModify = await _context.TransactorTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.TransTransactorDocSeries)
                .Include(t => t.TransTransactorDocType)
                .Include(t => t.Transactor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null) {
                return NotFound();
            }
            ItemVm = _mapper.Map<TransactorTransModifyDto>(transactionToModify);
            NotUpdatable = ItemVm.CreatorId != 0;
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id) {
            if (id == null) {
                return NotFound();
            }

            try
            {
                var serviceResult = await _transService.DeleteTransactorTransaction(id.Value);
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
            }
            catch (Exception ex)
            {
                string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
                ModelState.AddModelError(string.Empty, msg);
                LoadCombos();
                return Page();
            }

            return RedirectToPage("./Index");
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
            ViewData["CfAccountId"] = SelectListHelpers.GetCfAccountsNoSelectionList(_context);
        }
    }
}
