using System;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Sync;

namespace GrKouk.Web.ERP.Services;

public enum SupplierCodePushOutcome
{
    /// <summary>The hub did not know this key.</summary>
    Inserted,

    /// <summary>The hub's row changed and gets a new version.</summary>
    Updated,

    /// <summary>The shop agrees with the hub, and there is nothing to record.</summary>
    NoChange,

    /// <summary>A different mapping, from a shop that had not seen the current one. A conflict.</summary>
    Refused
}

/// <summary>
/// How the hub answers a pushed supplier code. Pure, so the rules can be read and tested on their own.
/// A conflict is never resolved here: a refused code changes nothing, and the shop keeps its own.
/// </summary>
public static class SupplierCodeHubRules
{
    public static bool IsValid(SupplierCodePushDto code) =>
        !string.IsNullOrWhiteSpace(code.SupplierVat) && code.SupplierVat.Length <= 20
        && !string.IsNullOrWhiteSpace(code.MatchKey) && code.MatchKey.Length <= 300
        && code.MatchKind is 1 or 2
        && code.ItemId != Guid.Empty
        && code.QuantityFactor > 0;

    /// <summary>The key's identity, the way the import compares keys.</summary>
    public static string LookupKey(string matchKey) => (matchKey ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    /// Applies a pushed code to the hub's row, or says why not. <paramref name="row"/> is null when the
    /// key is new; the caller then adds the returned row. The caller stamps a changed row's server time.
    /// </summary>
    public static SupplierCodePushOutcome Apply(
        SharedSupplierItemCode row, SupplierCodePushDto pushed, string shopId, out SharedSupplierItemCode result)
    {
        if (row == null)
        {
            result = new SharedSupplierItemCode
            {
                SupplierVat = pushed.SupplierVat,
                MatchKind = pushed.MatchKind,
                MatchKey = pushed.MatchKey,
                ItemId = pushed.ItemId,
                QuantityFactor = pushed.QuantityFactor,
                IsActive = pushed.IsActive,
                Source = pushed.Source,
                SupplierDescription = pushed.SupplierDescription,
                Version = 1,
                ModifiedAt = pushed.ModifiedAt,
                ModifiedByShopId = shopId
            };
            return SupplierCodePushOutcome.Inserted;
        }

        result = row;
        var knewCurrent = pushed.BaseVersion == row.Version;
        var sameMapping = row.ItemId == pushed.ItemId && row.QuantityFactor == pushed.QuantityFactor;

        if (sameMapping)
        {
            // They agree on what the code means, so what is left cannot conflict: the stronger source
            // stands, and the active flag follows whoever knew the current row or changed it last.
            var source = Math.Max(row.Source, pushed.Source);
            var isActive = knewCurrent || pushed.ModifiedAt >= row.ModifiedAt ? pushed.IsActive : row.IsActive;
            if (source == row.Source && isActive == row.IsActive) return SupplierCodePushOutcome.NoChange;

            row.Source = source;
            row.IsActive = isActive;
        }
        else
        {
            if (!knewCurrent) return SupplierCodePushOutcome.Refused;

            row.ItemId = pushed.ItemId;
            row.QuantityFactor = pushed.QuantityFactor;
            row.IsActive = pushed.IsActive;
            row.Source = pushed.Source;
        }

        if (pushed.SupplierDescription != null) row.SupplierDescription = pushed.SupplierDescription;
        row.Version++;
        row.ModifiedAt = pushed.ModifiedAt;
        row.ModifiedByShopId = shopId;
        return SupplierCodePushOutcome.Updated;
    }

    public static SharedSupplierCodeDto ToDto(SharedSupplierItemCode row) => new()
    {
        SupplierVat = row.SupplierVat,
        MatchKind = row.MatchKind,
        MatchKey = row.MatchKey,
        ItemId = row.ItemId,
        QuantityFactor = row.QuantityFactor,
        IsActive = row.IsActive,
        Source = row.Source,
        SupplierDescription = row.SupplierDescription,
        Version = row.Version,
        ModifiedAt = row.ModifiedAt,
        ModifiedByShopId = row.ModifiedByShopId
    };
}
