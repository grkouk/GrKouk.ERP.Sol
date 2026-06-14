using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// An inter-shop stock transfer published to the shared registry (the ERP-as-hub
/// transport for Feature A). The source shop inserts one row (+lines) after its
/// transfer-OUT document commits; the destination shop pulls it and materializes a
/// transfer-IN document. Immutable once inserted — transfers are FINAL (corrections
/// are a new reverse transfer), so there is no LWW/update path, only insert + a
/// one-way Status transition to Materialized when the dest acks.
///
/// Identity is <see cref="TransferId"/> (a GUID minted by the source). Items are
/// referenced by the shared Item GUID (<see cref="SharedStockTransferLine.ItemId"/>),
/// the same cross-shop key the item registry uses. The carried unit cost is the
/// source's average-cost snapshot at OUT-save time (see project plan).
/// </summary>
public class SharedStockTransfer
{
    [Key]
    public Guid TransferId { get; set; }

    /// <summary>OurShopId of the shop that originated (pushed) the transfer.</summary>
    [Required]
    [MaxLength(50)]
    public string SourceShopId { get; set; } = string.Empty;

    /// <summary>OurShopId of the receiving shop.</summary>
    [Required]
    [MaxLength(50)]
    public string DestShopId { get; set; } = string.Empty;

    /// <summary>The original transfer date — the dest IN doc must be dated to this.</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>Optional free-text note / reference.</summary>
    [MaxLength(200)]
    public string? Reference { get; set; }

    /// <summary>"Pushed" on insert; "Materialized" once the dest acks.</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = StockTransferStatuses.Pushed;

    /// <summary>Server receipt time (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the dest acknowledged materialization (UTC), else null.</summary>
    public DateTime? MaterializedAt { get; set; }

    public ICollection<SharedStockTransferLine> Lines { get; set; } = new List<SharedStockTransferLine>();
}

/// <summary>A single item line of a <see cref="SharedStockTransfer"/>.</summary>
public class SharedStockTransferLine
{
    [Key]
    public Guid Id { get; set; }

    public Guid TransferId { get; set; }
    public SharedStockTransfer? Transfer { get; set; }

    /// <summary>Shared Item GUID (cross-shop key), resolved to a local item at the dest.</summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Source item Code snapshot, carried for human-readable diagnostics (e.g. when the
    /// dest can't resolve <see cref="ItemId"/> because the item isn't synced there yet).
    /// Display-only — materialization still keys on <see cref="ItemId"/>.
    /// </summary>
    [MaxLength(50)]
    public string? ItemCode { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Source's average-cost snapshot carried to the dest as the IN unit cost.</summary>
    public decimal CarriedUnitCost { get; set; }

    /// <summary>Optional batch number supplied on the transfer line (like a purchase).</summary>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    /// <summary>Optional expiry supplied on the line; null = never-expiring at dest.</summary>
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>Status constants for <see cref="SharedStockTransfer.Status"/>.</summary>
public static class StockTransferStatuses
{
    public const string Pushed = "Pushed";
    public const string Materialized = "Materialized";
}
