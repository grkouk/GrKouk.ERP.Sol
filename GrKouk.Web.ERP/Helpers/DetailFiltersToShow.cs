namespace GrKouk.Web.ERP.Helpers;

public class DetailFiltersToShow
{
    public bool ShowDateFlt { get; set; } = true;
    public bool ShowCurrencyFlt { get; set; } = true;
    public bool ShowCompaniesFlt { get; set; } = false;
    public bool ShowCompaniesMultiFlt { get; set; } = true;
}