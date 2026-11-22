using System.Collections.Generic;

namespace GrKouk.Web.ERP.Helpers.SfGrid;

/// <summary>Model of Pages/Shared/Sf/_SfIndexToolbar: the links and menus above an index grid.</summary>
public class SfIndexToolbarModel
{
    /// <summary>Page of the "Create New" link; null hides it.</summary>
    public string CreatePage { get; set; } = "./Create";

    /// <summary>The related diaries; null or empty hides the diaries menu.</summary>
    public List<SfFilterItem> Diaries { get; set; }

    /// <summary>Page that runs a diary (gets <c>diaryId</c>).</summary>
    public string DiaryPage { get; set; }

    /// <summary>Show the row selectors toggle and the Actions (Delete Selected) menu.</summary>
    public bool RowSelection { get; set; } = true;
}

/// <summary>
/// Model of Pages/Shared/Sf/_SfIndexFilters: the search box plus the optional standard filters.
/// A null list hides that filter.
/// </summary>
public class SfIndexFiltersModel
{
    public List<SfFilterItem> DatePeriodItems { get; set; }
    public List<SfFilterItem> CompanyItems { get; set; }
    public List<SfCurrencyItem> CurrencyItems { get; set; }
    public string SearchPlaceholder { get; set; } = "Search...";
}

/// <summary>Model of Pages/Shared/Sf/_SfTotalsBar: page and grand totals under the grid.</summary>
public class SfTotalsBarModel
{
    /// <summary>The total columns. Each renders the cells "page{Key}" and "grand{Key}".</summary>
    public List<SfTotalColumn> Columns { get; set; } = new();
}

public record SfTotalColumn(string Key, string Header);

/// <summary>Model of Pages/Shared/Sf/_SfPaymentAssignModal (receipts or payments assignment).</summary>
public class SfPaymentAssignModalModel
{
    public string Title { get; set; }
}
