using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers
{
    public class IndexFilterResult
    {
        public List<SelectListItem> PageFilterSize { get; set; }
        public List<SelectListItem> CompanyFilter { get; set; }
        public List<SelectListItem> DataFilterValues { get; set; }
        public List<SelectListItem> CurrencySelector { get; set; }
    }

    public class IndexFiltersToShow
    {
        public bool ShowDateFlt { get; set; }
        public bool ShowMaterialNatureFlt { get; set; }
        public bool ShowTransactorTypeFlt { get; set; }
       // public bool ShowDateFlt { get; set; }
    }
}