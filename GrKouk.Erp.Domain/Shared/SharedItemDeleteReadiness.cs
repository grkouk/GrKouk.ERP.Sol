using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Per-shop readiness report for an item with a pending delete request
/// (SharedItem.DeleteRequested = true). One row per (ItemId, ShopId): it records
/// how many transaction rows that shop still holds referencing the item, and when
/// the count was last computed.
///
/// Filled by the bidirectional notification poll (POST /api/notifications/poll),
/// not by the full SharedItemSync — so readiness refreshes on the poll cadence
/// regardless of whether an operator runs a manual sync. The requesting shop's
/// "Finalize delete" button reads these rows: deletion is allowed only when every
/// shop reports LocalFkCount = 0 with a sufficiently fresh ReportedAt.
///
/// Composite primary key (ItemId, ShopId) configured in ApiDbContext.
/// </summary>
public class SharedItemDeleteReadiness
{
    /// <summary>The item the report is about (SharedItem.Id).</summary>
    public Guid ItemId { get; set; }

    /// <summary>The shop the report is from (its OurShopId).</summary>
    [Required]
    [MaxLength(50)]
    public string ShopId { get; set; } = string.Empty;

    /// <summary>
    /// Count of transaction rows on the reporting shop still referencing the item.
    /// Zero means that shop has nothing blocking a hard delete.
    /// </summary>
    public int LocalFkCount { get; set; }

    /// <summary>When the reporting shop last computed LocalFkCount (UTC).</summary>
    public DateTime ReportedAt { get; set; }
}
