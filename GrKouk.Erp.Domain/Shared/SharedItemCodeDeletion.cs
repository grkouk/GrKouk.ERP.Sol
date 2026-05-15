using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Tombstone row for a deleted ItemCode (barcode / supplier-code).
/// Pushed by the originating shop so the other shop can purge its matching
/// local row by Id. Applied BEFORE the upsert section of the same pull on
/// the receiver — that ordering frees the (CodeType, Code) unique-index slot
/// for an incoming new ItemCode that reuses the natural key.
///
/// Mirrors <see cref="SharedItemErpMappingDeletion"/>.
/// </summary>
public class SharedItemCodeDeletion
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the local ItemCode row that was deleted. Receiver hard-deletes
    /// any local ItemCodes row with this Id.
    /// </summary>
    public Guid DeletedItemCodeId { get; set; }

    public DateTime DeletedAt { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; } = string.Empty;
}
