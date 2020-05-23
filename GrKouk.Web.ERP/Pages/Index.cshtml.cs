using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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