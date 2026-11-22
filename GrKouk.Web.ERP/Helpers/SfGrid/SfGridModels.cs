using System;
using System.Collections.Generic;
using System.Globalization;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Helpers.SfGrid;

/// <summary>
/// Request of a Syncfusion grid (UrlAdaptor) plus the page filters, which the grid sends as
/// "params" (added with Query.addParams on the client).
/// </summary>
public class SfGridRequest : DataManagerRequest
{
    public Dictionary<string, object> Params { get; set; }

    public string GetString(string key) =>
        Params != null && Params.TryGetValue(key, out var value) && value != null
            ? Convert.ToString(value, CultureInfo.InvariantCulture)
            : null;

    public DateTime? GetDate(string key) =>
        DateTime.TryParse(GetString(key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d
            : null;

    public int? GetInt(string key) =>
        int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
            ? i
            : null;
}

/// <summary>Body of the "Delete Selected" request: the ids of the selected rows.</summary>
public class SfDeleteSelectedRequest
{
    public List<int> Ids { get; set; }
}

/// <summary>An item of a filter combo (value/text).</summary>
public class SfFilterItem
{
    public string Value { get; set; }
    public string Text { get; set; }
}

/// <summary>A display currency: the client formats amounts with its code and locale.</summary>
public class SfCurrencyItem
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayLocale { get; set; }
}

/// <summary>
/// The filters shared by the document index pages (period, company, search, display currency),
/// posted as the grid's params or as the body of the totals request.
/// </summary>
public class SfDocIndexFilters
{
    public string DateRange { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string CompanyFilter { get; set; }
    public string SearchFilter { get; set; }
    public int DisplayCurrencyId { get; set; } = 1;

    public static SfDocIndexFilters FromRequest(SfGridRequest dm) => new()
    {
        DateRange = dm.GetString("dateRange"),
        FromDate = dm.GetDate("fromDate"),
        ToDate = dm.GetDate("toDate"),
        CompanyFilter = dm.GetString("companyFilter"),
        SearchFilter = dm.GetString("searchFilter"),
        DisplayCurrencyId = dm.GetInt("displayCurrencyId") is > 0 and var c ? c : 1
    };
}
