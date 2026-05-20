using System;
using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.Sync;

// ─── Bidirectional notification poll (Workstream B/C) ───────────────
//
// A shop polls POST /api/notifications/poll on a timer (default 60 min),
// independent of the full SharedItemSync. The request carries this shop's
// current readiness rows for every DeleteRequested item; the response carries
// the cross-shop pending-delete summary. The same call bumps KnownShops.
// LastSeenAt, doubling as a liveness heartbeat.

/// <summary>
/// Per-shop readiness report for an item with a pending delete request.
/// Wire form of <c>SharedItemDeleteReadiness</c> / <c>ItemDeleteReadiness</c>.
/// </summary>
public class ItemDeleteReadinessDto
{
    public Guid ItemId { get; set; }
    public string ShopId { get; set; } = string.Empty;
    public int LocalFkCount { get; set; }
    public DateTime ReportedAt { get; set; }
}

/// <summary>
/// Poll request: the calling shop's readiness for its DeleteRequested items.
/// </summary>
public class NotificationPollRequest
{
    public string ShopId { get; set; } = string.Empty;
    public List<ItemDeleteReadinessDto> Readiness { get; set; } = new();
}

/// <summary>
/// One item with a pending delete request, plus every shop's readiness for it.
/// </summary>
public class PendingDeleteItemDto
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string RequestedByShopId { get; set; } = string.Empty;
    public DateTime? RequestedAt { get; set; }

    /// <summary>True when the polling shop's own readiness row reports LocalFkCount = 0.</summary>
    public bool ThisShopReady { get; set; }

    /// <summary>True when every active shop has reported LocalFkCount = 0.</summary>
    public bool AllShopsReady { get; set; }

    /// <summary>Every shop's readiness row for this item (for banner / management view).</summary>
    public List<ItemDeleteReadinessDto> Readiness { get; set; } = new();
}

/// <summary>
/// Poll response: the cross-shop notification summary. Extensible — future
/// notification kinds (unresolved conflicts, price changes) add fields here.
/// </summary>
public class NotificationSummaryDto
{
    public DateTime AsOf { get; set; }
    public int PendingDeleteCount { get; set; }
    public List<PendingDeleteItemDto> PendingDeletes { get; set; } = new();
}
