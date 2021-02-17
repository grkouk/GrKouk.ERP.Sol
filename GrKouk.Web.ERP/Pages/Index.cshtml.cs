using System.Linq;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            LoadFilters();
        }

        private void LoadFilters()
        {
            var currencyListJs = _context.Currencies.OrderBy(p => p.Name).AsNoTracking().ToList();
            ViewData["CurrencyListJs"] = currencyListJs;

            var companiesListJs = FiltersHelper.GetCompaniesFilterList(_context);
            ViewData["CompanyListJs"] = companiesListJs;

            var datePeriodListJs = DateFilter.GetDateFiltersSelectList();
            ViewData["DatePeriodListJs"] = datePeriodListJs;
        }
    }
}