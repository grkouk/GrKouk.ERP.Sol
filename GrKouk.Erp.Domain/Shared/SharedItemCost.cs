using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Per-shop average cost published to the shared registry. One row per
/// (ShopId, ItemId): the cost that shop has computed locally for an item it
/// actually purchased. Lets ANOTHER shop borrow this value for items it sells
/// but never bought (stock moved between shops without paperwork), purely for
/// COGS / gross-profit analysis in its sale summary diary.
///
/// Deliberately separate from <see cref="SharedItem"/> (the LWW item row): a
/// purchase changes AverageCost without bumping Item.ModifiedAt, so cost can
/// never ride the item channel; and keeping it off the LWW row avoids any echo
/// into the conflict machinery. Pushed during sync, read on demand via
/// GET /api/sharedsync/itemcost/{itemId}.
///
/// Identity is the shared Item GUID (both shops start from the same DB clone),
/// scoped per shop via <see cref="ShopId"/>. Composite primary key
/// (ShopId, ItemId) configured in ApiDbContext.
/// </summary>
public class SharedItemCost
{
    /// <summary>The shop that published this cost (its OurShopId).</summary>
    [Required]
    [MaxLength(50)]
    public string ShopId { get; set; } = string.Empty;

    /// <summary>The item the cost is for (SharedItem.Id).</summary>
    public Guid ItemId { get; set; }

    /// <summary>The publishing shop's locally-computed weighted-average cost.</summary>
    public decimal AverageCost { get; set; }

    /// <summary>The publishing shop's last purchase price for the item.</summary>
    public decimal LastPurchasePrice { get; set; }

    /// <summary>When the publishing shop last updated this cost (UTC). LWW key.</summary>
    public DateTime UpdatedAt { get; set; }
}
