using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

public class SharedItemCode
{
    [Key]
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public int CodeType { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; }

    public Guid MeasureUnitId { get; set; }

    public decimal Quantity { get; set; } = 1;

    /// <summary>Earliest known creation of this code across shops (MIN-merged on push-in). Null = unknown/legacy.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Latest scan-commit of this code across shops (MAX-merged on push-in; the server is
    /// the convergence hub). Null = never observed. A stamp changes no other column, so pulls
    /// cannot select on parent-item recency alone: they select on <see cref="ReceivedAt"/>.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Hub clock (UTC) when a push last added this code or really changed it (a field or a newer
    /// <see cref="LastUsedAt"/>). The pull asks by it, as for the other shared rows, so a scan stamp
    /// a shop pushes late still reaches the other shops. Hub only: never sent to a shop.
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>Shop whose push set <see cref="ReceivedAt"/>; the pull does not send the change back to it. Null = backfill or catch-up.</summary>
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; }
}
