using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Records that a specific shop has applied a specific tombstone. Used by the
/// server's purge pass to delete a tombstone once every active KnownShop has
/// acked. Idempotent via UNIQUE INDEX on (TombstoneId, TombstoneType, ShopId).
///
/// TombstoneType values map to the four tombstone tables:
///   "ItemCode"             -> SharedItemCodeDeletions
///   "ItemPriceLevelMapping" -> SharedItemPriceLevelMappingDeletions
///   "ItemErpMapping"       -> SharedItemErpMappingDeletions
///   "Item"                 -> SharedItemDeletions (Workstream B)
/// See <see cref="TombstoneTypes"/> for the canonical constants.
/// </summary>
public class SharedTombstoneAck
{
    [Key]
    public Guid Id { get; set; }

    public Guid TombstoneId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TombstoneType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ShopId { get; set; } = string.Empty;

    public DateTime AckedAt { get; set; }
}

/// <summary>
/// Canonical string values for SharedTombstoneAck.TombstoneType.
/// </summary>
public static class TombstoneTypes
{
    public const string ItemCode = "ItemCode";
    public const string ItemPriceLevelMapping = "ItemPriceLevelMapping";
    public const string ItemErpMapping = "ItemErpMapping";
    public const string Item = "Item";
}
