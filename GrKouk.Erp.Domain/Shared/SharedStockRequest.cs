using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// An inter-shop stock REQUEST published to the shared registry: a shop that wants goods
/// broadcasts what it needs, and any other shop may fulfill part of it. Unlike
/// <see cref="SharedStockTransfer"/> (an immutable fact), this row is LIVE STATE and the ERP
/// is its single source of truth: remaining quantity is only ever changed here, atomically,
/// by the synchronous claim / release / cancel-remainder endpoints — never by push upserts.
///
/// Lifecycle: Open → Completed (every line's raw remaining ≤ 0, whether by fulfillment,
/// remainder-cancel, or a mix) | Cancelled (requester cancelled before ANY fulfillment).
/// Closed requests are kept for history until the ERP user clears them.
///
/// A request has ZERO inventory/financial effect; goods move only via the
/// <see cref="SharedStockTransfer"/>s that ship its fulfillments (linked by ClaimId).
/// </summary>
public class SharedStockRequest
{
    [Key]
    public Guid RequestId { get; set; }

    /// <summary>OurShopId of the shop that wants the goods.</summary>
    [Required]
    [MaxLength(50)]
    public string RequestingShopId { get; set; } = string.Empty;

    /// <summary>Null = broadcast (any shop may fulfill). Reserved for future targeted requests.</summary>
    [MaxLength(50)]
    public string? TargetShopId { get; set; }

    /// <summary>Discriminator reserved for future request kinds; always "Stock" for now.</summary>
    [Required]
    [MaxLength(20)]
    public string RequestType { get; set; } = StockRequestTypes.Stock;

    public DateTime RequestDate { get; set; }

    /// <summary>Optional free-text note / reference.</summary>
    [MaxLength(200)]
    public string? Reference { get; set; }

    /// <summary>"Open" on insert; see <see cref="StockRequestStatuses"/>.</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = StockRequestStatuses.Open;

    /// <summary>Server receipt time (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Bumped by every state change (claim/release/cancel/complete) — drives the requester's
    /// since-filtered progress pull and the fulfillers' since-filtered close signal.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ICollection<SharedStockRequestLine> Lines { get; set; } = new List<SharedStockRequestLine>();

    public ICollection<SharedStockRequestFulfillment> Fulfillments { get; set; } =
        new List<SharedStockRequestFulfillment>();
}

/// <summary>A single item line of a <see cref="SharedStockRequest"/>. Remaining is always
/// derived as max(0, Requested − Fulfilled − Cancelled), never stored; the line is complete
/// when the RAW remaining is ≤ 0.</summary>
public class SharedStockRequestLine
{
    [Key]
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }
    public SharedStockRequest? Request { get; set; }

    /// <summary>Shared Item GUID (cross-shop key), the same key the item registry uses.</summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Requester item Code snapshot, carried for human-readable diagnostics (e.g. when a
    /// fulfiller can't resolve <see cref="ItemId"/> because the item isn't synced there yet).
    /// Display-only — fulfillment still keys on <see cref="ItemId"/>.
    /// </summary>
    [MaxLength(50)]
    public string? ItemCode { get; set; }

    public decimal RequestedQuantity { get; set; }

    /// <summary>
    /// Accumulated atomically by claims; MAY exceed <see cref="RequestedQuantity"/> when the
    /// fulfiller explicitly confirmed over-fulfillment (pack/carton rounding).
    /// </summary>
    public decimal FulfilledQuantity { get; set; }

    /// <summary>Remainder written off by cancel-remainder = max(0, requested − fulfilled) at
    /// that moment; never negative.</summary>
    public decimal CancelledQuantity { get; set; }
}

/// <summary>
/// One shop's claim against a <see cref="SharedStockRequest"/> — the audit of who ships what.
/// Inserted atomically by the claim endpoint as Claimed (quantity reserved); flipped to
/// Shipped when the pushed <see cref="SharedStockTransfer"/> carrying this ClaimId arrives;
/// Released by the fulfiller's compensating call or an ERP-admin void of a stuck claim
/// (flag-only after N days — NEVER auto-released, goods may have shipped with the push
/// merely delayed).
/// </summary>
public class SharedStockRequestFulfillment
{
    [Key]
    public Guid ClaimId { get; set; }

    public Guid RequestId { get; set; }
    public SharedStockRequest? Request { get; set; }

    /// <summary>OurShopId of the fulfilling shop — forced to the authenticated caller.</summary>
    [Required]
    [MaxLength(50)]
    public string FulfillingShopId { get; set; } = string.Empty;

    /// <summary>"Claimed" on insert; see <see cref="StockRequestFulfillmentStatuses"/>.</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = StockRequestFulfillmentStatuses.Claimed;

    /// <summary>The <see cref="SharedStockTransfer.TransferId"/> that shipped this claim,
    /// stamped when it arrives on the push; null while Claimed / after Released.</summary>
    public Guid? TransferId { get; set; }

    /// <summary>Server receipt time of the claim (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public ICollection<SharedStockRequestFulfillmentLine> Lines { get; set; } =
        new List<SharedStockRequestFulfillmentLine>();
}

/// <summary>A single item line of a <see cref="SharedStockRequestFulfillment"/> — the granted
/// quantity that was added to the request line's FulfilledQuantity (and is subtracted back on
/// release).</summary>
public class SharedStockRequestFulfillmentLine
{
    [Key]
    public Guid Id { get; set; }

    public Guid ClaimId { get; set; }
    public SharedStockRequestFulfillment? Fulfillment { get; set; }

    /// <summary>Shared Item GUID (cross-shop key).</summary>
    public Guid ItemId { get; set; }

    public decimal Quantity { get; set; }
}

/// <summary>Status constants for <see cref="SharedStockRequest.Status"/>.</summary>
public static class StockRequestStatuses
{
    public const string Open = "Open";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

/// <summary>Status constants for <see cref="SharedStockRequestFulfillment.Status"/>.</summary>
public static class StockRequestFulfillmentStatuses
{
    public const string Claimed = "Claimed";
    public const string Shipped = "Shipped";
    public const string Released = "Released";
}

/// <summary>Request-type constants for <see cref="SharedStockRequest.RequestType"/>.</summary>
public static class StockRequestTypes
{
    public const string Stock = "Stock";
}
