using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.TransactorTransMng
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        public bool NotUpdatable;
        public DeleteModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty]
        public TransactorTransModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
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

            if (transactionToModify == null)
            {
                return NotFound();
            }
            ItemVm = _mapper.Map<TransactorTransModifyDto>(transactionToModify);
            NotUpdatable = ItemVm.CreatorId != 0;
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemToDelete = await _context.TransactorTransactions.FindAsync(id);

            if (itemToDelete != null)
            {
                _context.BuyDocTransPaymentMappings.RemoveRange(
                    _context.BuyDocTransPaymentMappings
                        .Where(p => p.TransactorTransactionId == itemToDelete.Id));
                _context.SellDocTransPaymentMappings.RemoveRange(
                    _context.SellDocTransPaymentMappings
                        .Where(p => p.TransactorTransactionId == itemToDelete.Id));

                _context.TransactorTransactions.Remove(itemToDelete);
                await _context.SaveChangesAsync();
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

                transactorsList.Add(new SelectListItem() { Value = dbTransactor.Id.ToString(), Text = dbTransactor.Name + "-" + dbTransactor.TransactorType.Code });
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["FiscalPeriodId"] = new SelectList(_context.FiscalPeriods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorsList, "Value", "Text");
            ViewData["TransTransactorDocSeriesId"] = new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            //ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Code");
        }
    }
}
