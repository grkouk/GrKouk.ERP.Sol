using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.ViewComponents;

public class DetailFiltersViewComponent:ViewComponent
{
    private readonly ApiDbContext _context;

    public DetailFiltersViewComponent(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(DetailFiltersToShow filtersToShow)
    {
        var detailFiltersResult = new DetailFilterResult();
        if (filtersToShow.ShowCompaniesFlt)
        {
            detailFiltersResult.CompanyFilterValues = await FiltersHelper.GetCompaniesFilterListAsync(_context);
        }

        if (filtersToShow.ShowCompaniesMultiFlt)
        {
            detailFiltersResult.CompaniesFilterUiSelectItems = await FiltersHelper.GetCompaniesFilterUiListAsync(_context);
        }
        if (filtersToShow.ShowCurrencyFlt)
        {
            detailFiltersResult.CurrencyFilterValues = await FiltersHelper.GetCurrenciesFilterListAsync(_context);
        }
        if (filtersToShow.ShowDateFlt)
        {
            detailFiltersResult.DateFilterValues = DateFilter.GetDateFiltersSelectList();
        }
        detailFiltersResult.FiltersToShow = filtersToShow;
        ViewData["ViewContext"] = ViewContext; 
        return View(detailFiltersResult);
    }
}