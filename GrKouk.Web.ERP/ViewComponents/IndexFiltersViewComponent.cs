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
        
        public async Task<IViewComponentResult> InvokeAsync( IndexFiltersToShow filtersToShow)
        {

            var indexFiltersResult = new IndexFilterResult();
            if (filtersToShow.ShowCompaniesFlt)
            {
                indexFiltersResult.CompanyFilterValues = await FiltersHelper.GetCompaniesFilterListAsync(_context);
            }
            if (filtersToShow.ShowCurrencyFlt)
            {
                indexFiltersResult.CurrencyFilterValues = await FiltersHelper.GetCurrenciesFilterListAsync(_context);
            }
            if (filtersToShow.ShowDateFlt)
            {
                indexFiltersResult.DateFilterValues = DateFilter.GetDateFiltersSelectList();
            }
            if (filtersToShow.ShowPageSizeFlt)
            {
                indexFiltersResult.PageSizeFilterValues = FiltersHelper.GetPageSizeFiltersSelectList();
            }
            if (filtersToShow.ShowMaterialNatureFlt)
            {
                indexFiltersResult.MaterialNaturesFilterValues = FiltersHelper.GetWarehouseItemNaturesList();
            }
            if (filtersToShow.ShowTransactorTypeFlt)
            {
                indexFiltersResult.TransactorTypeFilterValues = await FiltersHelper.GetTransactorTypeFilterListAsync(_context);
            }

            indexFiltersResult.FiltersToShow = filtersToShow;
            return View(indexFiltersResult);
        }
    }
}