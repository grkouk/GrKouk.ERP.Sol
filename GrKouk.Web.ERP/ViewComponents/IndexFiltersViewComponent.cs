using System;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.ViewComponents
{
    public class IndexFiltersViewComponent:ViewComponent
    {
        private readonly ApiDbContext _context;

        public IndexFiltersViewComponent(ApiDbContext context)
        {
            _context = context;
        }
        
        public async Task<IViewComponentResult> InvokeAsync( IndexFiltersToShow filtersToShow
            )
        {
            // Console.WriteLine(showDateFlt);
            // Console.WriteLine(showMaterialNatureFlt);
            // Console.WriteLine(showTransactorTypeFlt);
            Console.WriteLine(filtersToShow.ShowDateFlt);
            Console.WriteLine(filtersToShow.ShowMaterialNatureFlt);
            Console.WriteLine(filtersToShow.ShowTransactorTypeFlt);
            
            var indexFiltersResult = new IndexFilterResult
            {
                CompanyFilter = FiltersHelper.GetCompaniesFilterList(_context),
                CurrencySelector = FiltersHelper.GetCurrenciesFilterList(_context),
                DataFilterValues = DateFilter.GetDateFiltersSelectList(),
                PageFilterSize = FiltersHelper.GetPageSizeFiltersSelectList()
            };
            return View(indexFiltersResult);
        }
    }
}