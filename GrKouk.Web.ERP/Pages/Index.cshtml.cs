using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadFiltersAsync();
            return Page();
        }
        private async Task LoadFiltersAsync()
        {
            Func<Task<List<UISelectTypeItem>>> companiesListJsFunc = async () =>
            {
                var itemsList = await _context.Companies
                    .OrderBy(p => p.Code)
                    .Select(p => new UISelectTypeItem()
                    {
                        Title = p.Code,
                        ValueInt = p.Id,
                        Value = p.Id.ToString()
                    })
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<Currency>>> currenciesListJsFunc = async () =>
            {
                var itemsList = await _context.Currencies
                    .OrderBy(p => p.Name)
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };

            ViewData["CurrencyListJs"] = await currenciesListJsFunc();
            ViewData["DatePeriodListJs"] = DateFilter.GetDateFiltersSelectList();
            ViewData["CompaniesListJs"] = await companiesListJsFunc();
            ViewData["AllCompaniesId"] = await FiltersHelper.GetAllCompaniesIdAsync(_context);
        }
        // private void LoadFilters()
        // {
        //     var currencyListJs = _context.Currencies.OrderBy(p => p.Name).AsNoTracking().ToList();
        //     ViewData["CurrencyListJs"] = currencyListJs;
        //
        //     var companiesListJs = FiltersHelper.GetCompaniesFilterList(_context);
        //     ViewData["CompanyListJs"] = companiesListJs;
        //
        //     var datePeriodListJs = DateFilter.GetDateFiltersSelectList();
        //     ViewData["DatePeriodListJs"] = datePeriodListJs;
        // }
    }
}