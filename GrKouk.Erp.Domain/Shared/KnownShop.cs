using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Registry of every shop that has ever pushed to this server. Auto-populated
/// on the first push from each shop. Used by the tombstone purge pass to
/// determine "all shops have acked" — purge fires once every IsActive=true
/// row has a matching SharedTombstoneAck for a given tombstone.
///
/// An admin can flip IsActive=false to decommission a shop (unblocks purges
/// that were waiting on its ack).
/// </summary>
public class KnownShop
{
    [Key]
    [MaxLength(50)]
    public string ShopId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public DateTime FirstSeenAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public bool IsActive { get; set; } = true;
}
