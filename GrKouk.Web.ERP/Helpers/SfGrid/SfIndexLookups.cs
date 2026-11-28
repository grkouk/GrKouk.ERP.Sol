using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers.SfGrid;

/// <summary>Lookup lists and small helpers shared by the Syncfusion index pages.</summary>
public static class SfIndexLookups
{
    public static async Task<List<SfFilterItem>> GetCompanyItemsAsync(ApiDbContext context) =>
        (await FiltersHelper.GetCompaniesFilterListAsync(context))
        .Select(p => new SfFilterItem { Value = p.Value, Text = p.Text })
        .ToList();

    /// <summary>Status filter items: "" all, "1" active, "0" inactive.</summary>
    public static List<SfFilterItem> GetActiveItems() => new()
    {
        new SfFilterItem { Value = "", Text = "{All}" },
        new SfFilterItem { Value = "1", Text = "Active" },
        new SfFilterItem { Value = "0", Text = "Inactive" }
    };

    /// <summary>The period list. The shared list repeats some "last year" months; a combo needs unique values.</summary>
    public static List<SfFilterItem> GetDatePeriodItems() =>
        DateFilter.GetDateFiltersSelectList()
            .DistinctBy(p => p.Value)
            .Select(p => new SfFilterItem { Value = p.Value, Text = p.Text })
            .ToList();

    public static Task<List<SfCurrencyItem>> GetCurrencyItemsAsync(ApiDbContext context) =>
        context.Currencies.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new SfCurrencyItem
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                DisplayLocale = p.DisplayLocale
            })
            .ToListAsync();

    public static Task<List<SfFilterItem>> GetDiaryItemsAsync(ApiDbContext context, DiaryTypeEnum diaryType) =>
        context.DiaryDefs.AsNoTracking()
            .Where(p => p.DiaryType == diaryType)
            .OrderBy(p => p.Name)
            .Select(p => new SfFilterItem { Value = p.Id.ToString(), Text = p.Name })
            .ToListAsync();

    /// <summary>The latest exchange rates, for converting amounts to the display currency.</summary>
    public static Task<List<ExchangeRate>> GetRecentRatesAsync(ApiDbContext context) =>
        context.ExchangeRates.AsNoTracking()
            .OrderByDescending(p => p.ClosingDate)
            .Take(10)
            .ToListAsync();

    private static readonly string[] DayAbbreviations = { "Κυ", "Δε", "Τρ", "Τε", "Πε", "Πα", "Σα" };

    /// <summary>Grid date text: two-letter Greek day name plus date, e.g. "Δε 27/12/2026".</summary>
    public static string FormatGridDate(DateTime date) =>
        $"{DayAbbreviations[(int)date.DayOfWeek]} {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
}
