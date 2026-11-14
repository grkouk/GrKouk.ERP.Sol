using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.FinDiary;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Expenses
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;


        public EditModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public FinDiaryExpenceTransModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var diaryTransactionToModify = await _context.FinDiaryTransactions
                .Include(f => f.Company)
                .Include(f => f.CostCentre)
                .Include(f => f.FinTransCategory)
                .Include(f => f.RevenueCentre)
                .Include(f => f.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (diaryTransactionToModify == null)
            {
                return NotFound();
            }

            ItemVm = diaryTransactionToModify.ToExpenseModifyDto();

            LoadCompbos();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCompbos();
                return Page();
            }

            var diaryTransactionToAttach = await _context.FinDiaryTransactions
                .FirstOrDefaultAsync(t => t.Id == ItemVm.Id);
            if (diaryTransactionToAttach == null)
            {
                return NotFound();
            }
            _context.Entry(diaryTransactionToAttach).Property(t => t.Timestamp).OriginalValue = ItemVm.Timestamp;
            ItemVm.ApplyTo(diaryTransactionToAttach);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FinDiaryTransactionExists(diaryTransactionToAttach.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToPage("./Index");
        }

        private bool FinDiaryTransactionExists(int id)
        {
            return _context.FinDiaryTransactions.Any(e => e.Id == id);
        }

        private void LoadCompbos()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["CostCentreId"] = new SelectList(_context.CostCentres.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
            ViewData["FinTransCategoryId"] = new SelectList(_context.FinTransCategories.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
            // ViewData["RevenueCentreId"] = new SelectList(_context.RevenueCentres.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
            var transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.DTRANSACTOR").OrderBy(s => s.Name).AsNoTracking();
            ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");

        }
    }
}
