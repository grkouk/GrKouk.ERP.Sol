using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Tombstone row for a deleted ItemPriceLevelMapping. Pushed by the
/// originating shop so the other shop can purge its matching local row by Id.
/// Applied BEFORE the upsert section of the same pull on the receiver —
/// frees the (ItemId, PriceLevelId) unique-index slot for incoming new rows.
///
/// Phase 1 ships the schema + DTOs + handlers; no UI path produces these on
/// the cash register side today (only cascade via DeleteCopiedItemAsync, which
/// isn't tombstoned yet). The push/pull channel is open so the other shop can
/// start producing them when its own Phase 1 ships.
/// </summary>
public class SharedItemPriceLevelMappingDeletion
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the local ItemPriceLevelMapping row that was deleted. Receiver
    /// hard-deletes any local mapping with this Id.
    /// </summary>
    public Guid DeletedMappingId { get; set; }

    public DateTime DeletedAt { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; } = string.Empty;
}
