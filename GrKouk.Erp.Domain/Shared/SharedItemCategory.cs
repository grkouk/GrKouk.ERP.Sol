using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

public class SharedItemCategory : IHubReceivedStamp
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// Batch-tracked items of this category need an expiry date. Null = no shop that knows the
    /// field has pushed this category yet; the pair is then unknown and the receiving shop keeps
    /// its own values.
    /// </summary>
    public bool? RequiresExpiry { get; set; }

    /// <summary>Category shelf life in days (expiry estimate). Meaningful only when <see cref="RequiresExpiry"/> is set.</summary>
    public int? ShelfLifeDays { get; set; }

    public DateTime ModifiedAt { get; set; }

    /// <summary>Hub clock when this change was stored; the pull filters by it.</summary>
    public DateTime ReceivedAt { get; set; }
}
