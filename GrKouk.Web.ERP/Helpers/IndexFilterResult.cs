using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers
{
    public class IndexFilterResult
    {
        public List<SelectListItem> PageSizeFilterValues { get; set; }
        public List<SelectListItem> CompanyFilterValues { get; set; }
        public List<SelectListItem> DateFilterValues { get; set; }
        public List<SelectListItem> CurrencyFilterValues { get; set; }
        public List<SelectListItem> MaterialNaturesFilterValues { get; set; }
        public List<SelectListItem> TransactorTypeFilterValues { get; set; }
        public IndexFiltersToShow FiltersToShow { get; set; }
    }

    public class IndexFiltersToShow
    {
        public bool ShowDateFlt { get; set; } = true;
        public bool ShowCurrencyFlt { get; set; } = true;
        public bool ShowMaterialNatureFlt { get; set; } = false;

        public bool ShowTransactorTypeFlt { get; set; } = false;
        public bool ShowCompaniesFlt { get; set; } = true;
        public bool ShowPageSizeFlt { get; set; } = true;
        public bool ShowDisplayCarryOnLineFlt { get; set; } = false;
        public bool ShowDisplaySummaryFlt { get; set; } = false;
        public bool ShowDisplayLinesWithZeroAmountFlt { get; set; } = false;
    }
}