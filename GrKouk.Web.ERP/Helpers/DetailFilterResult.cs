using System.Collections.Generic;
using GrKouk.Erp.Dtos.Diaries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers;

public class DetailFilterResult
{
    public List<SelectListItem> PageSizeFilterValues { get; set; }
    public List<SelectListItem> CompanyFilterValues { get; set; }
    public List<UISelectTypeItem> CompaniesFilterUiSelectItems { get; set; }
    public List<SelectListItem> DateFilterValues { get; set; }
    public List<SelectListItem> CurrencyFilterValues { get; set; }
    public List<SelectListItem> MaterialNaturesFilterValues { get; set; }
    public List<SelectListItem> TransactorTypeFilterValues { get; set; }
    public List<SelectListItem> MaterialCategoriesFilterValues { get; set; }
    public List<SelectListItem> SectionsFilterValues { get; set; }
    public List<UISelectTypeItem> SectionsFilterUiSelectItems { get; set; }
    public DetailFiltersToShow FiltersToShow { get; set; }
       
}