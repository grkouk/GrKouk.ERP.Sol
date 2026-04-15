using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Tombstone row for a deleted ItemErpMapping. Pushed by the originating shop
/// so the other shop can purge its matching local mapping. Rows age out of
/// the registry once both shops have pulled past their ModifiedAt.
/// </summary>
public class SharedItemErpMappingDeletion
{
    [Key]
    public Guid Id { get; set; }

    public Guid LocalItemId { get; set; }

    public DateTime DeletedAt { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; } = string.Empty;
}
