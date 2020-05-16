using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.FinDiary;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Expenses
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public int  CopyFromId { get; set; }

        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync(int? copyFromId)
        {

            LoadCombos();
            CopyFromId = 0;

            if (copyFromId != null)
            {
                CopyFromId = (int) copyFromId;
                var diaryTransactionToModify = await _context.FinDiaryTransactions
                    .Include(f => f.Company)
                    .ThenInclude(f=>f.Currency)
                    .Include(f => f.CostCentre)
                    .Include(f => f.FinTransCategory)
                    .Include(f => f.RevenueCentre)
                    .Include(f => f.Transactor).FirstOrDefaultAsync(m => m.Id == CopyFromId);

                if (diaryTransactionToModify != null)
                {
                    FinDiaryTransaction = _mapper.Map<FinDiaryExpenceTransModifyDto>(diaryTransactionToModify);
                }
                
            }

            return Page();
        }

        [BindProperty]
        public FinDiaryExpenceTransModifyDto FinDiaryTransaction { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
                return Page();
            }

            var diaryTransactionToAttach = _mapper.Map<FinDiaryTransaction>(FinDiaryTransaction);

            diaryTransactionToAttach.Kind = (int)DiaryTransactionsKindEnum.Expence;
            diaryTransactionToAttach.RevenueCentreId = 1;

            _context.FinDiaryTransactions.Add(diaryTransactionToAttach);
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Saved!");
            return RedirectToPage("./Index");
        }

        private void LoadCombos()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(c => c.Code).AsNoTracking(), "Id", "Code");
            ViewData["CostCentreId"] = new SelectList(_context.CostCentres.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
            ViewData["FinTransCategoryId"] = new SelectList(_context.FinTransCategories.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
           // ViewData["RevenueCentreId"] = new SelectList(_context.RevenueCentres.OrderBy(c => c.Name).AsNoTracking(), "Id", "Name");
            var transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.DTRANSACTOR").OrderBy(s => s.Name).AsNoTracking();
            ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");

            var companyCurrencyListJs = _context.Companies.Include(p=>p.Currency).OrderBy(p => p.Id).AsNoTracking()
                .Select(p => new CompanyCurrencyList()
                {
                    CompanyId = p.Id,
                    CurrencyId = p.Currency.Id,
                    CurrencyCode = p.Currency.Code,
                    DisplayLocale = p.Currency.DisplayLocale,
                    CurrencyName = p.Currency.Name
                    
                }).ToList();
            ViewData["CompanyCurrencyListJs"] = companyCurrencyListJs;
        }
    }
}