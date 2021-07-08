using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.CashFlowAccounts
{
    [Authorize(Roles = "Admin")]
    public class KartelaModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public string TransactorName { get; set; }
        public int TransactorId { get; set; }

        public KartelaModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }


        public async Task<IActionResult> OnGetAsync(int transactorId)
        {
            var transactor = await _context.Transactors.FirstOrDefaultAsync(x => x.Id == transactorId);
            if (transactor is null)
            {
                return NotFound();
            }

            TransactorId = transactorId;
            TransactorName = transactor.Name;
            LoadFilters();
            return Page();
        }
        private void LoadFilters()
        {

            var datePeriods = DateFilter.GetDateFiltersSelectList();
            ViewData["DataFilterValues"] = new SelectList(datePeriods, "Value", "Text");

            var pageFilterSize = PageFilter.GetPageSizeFiltersSelectList();
            ViewData["PageFilterSize"] = new SelectList(pageFilterSize, "Value", "Text");

            var companiesList = FiltersHelper.GetCompaniesFilterList(_context);
            ViewData["CompanyFilter"] = new SelectList(companiesList, "Value", "Text");
            ViewData["CurrencySelector"] = new SelectList(FiltersHelper.GetCurrenciesFilterList(_context), "Value", "Text");
            var currencyListJs = _context.Currencies.OrderBy(p => p.Id).AsNoTracking().ToList();
            ViewData["CurrencyListJs"] = currencyListJs;
        }
    }
}