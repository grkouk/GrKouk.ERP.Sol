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
    /// the convergence hub). Null = never observed. Also the pull-out watermark: a stamp
    /// changes no other column, so pulls must select on it, not just on parent-item recency.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}
