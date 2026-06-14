using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Per-shop stock snapshot published to the shared registry. One row per
/// (ShopId, ItemId): the on-hand quantity and weighted-average cost that shop
/// holds for an item. Lets ANOTHER shop see how much stock a peer holds, purely
/// as a decision aid for inter-shop transfers ("Άλλα καταστήματα" panel in
/// ItemInfoDialog). Read-only, advisory — "good enough to decide a transfer,"
/// not real-time.
///
/// Deliberately separate from <see cref="SharedItemCost"/>: that row carries cost
/// only for items a shop actually PURCHASED (AverageCost &gt; 0 plus a buy line),
/// gating the cost-borrow feature; this row carries raw on-hand stock for every
/// item the shop has had inventory activity for, regardless of cost basis. Pushed
/// on the normal sync cycle (delta by InventoryTransaction.CreatedAt watermark),
/// read on demand via GET /api/sharedsync/stockacrossshops/{itemId}.
///
/// Identity is the shared Item GUID (both shops start from the same DB clone),
/// scoped per shop via <see cref="ShopId"/>. Composite primary key
/// (ShopId, ItemId) configured in ApiDbContext. UpdatedAt is the LWW key.
/// </summary>
public class SharedInventory
{
    /// <summary>The shop that published this snapshot (its OurShopId).</summary>
    [Required]
    [MaxLength(50)]
    public string ShopId { get; set; } = string.Empty;

    /// <summary>The item the snapshot is for (SharedItem.Id).</summary>
    public Guid ItemId { get; set; }

    /// <summary>The publishing shop's on-hand quantity for the item.</summary>
    public decimal StockQuantity { get; set; }

    /// <summary>The publishing shop's locally-computed weighted-average cost.</summary>
    public decimal AverageCost { get; set; }

    /// <summary>When the publishing shop last updated this snapshot (UTC). LWW key.</summary>
    public DateTime UpdatedAt { get; set; }
}
