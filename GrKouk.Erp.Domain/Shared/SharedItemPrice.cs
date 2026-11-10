using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Shared item price for syncing inter-shop price level prices between shops.
/// Uses the local GUID as the shared identity (both shops start from the same DB clone).
/// </summary>
public class SharedItemPrice : IHubReceivedStamp
{
    [Key]
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public Guid PriceLevelId { get; set; }

    public decimal NetPrice { get; set; }

    public decimal BrutPrice { get; set; }

    public decimal Markup { get; set; }

    public bool IsOverridden { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    /// <summary>Hub clock when this change was stored; the pull filters by it.</summary>
    public DateTime ReceivedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; }
}
