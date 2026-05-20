using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Tombstone row for a hard-deleted item (Workstream B, two-phase delete).
/// Created by the requesting shop only after the cross-shop readiness negotiation
/// confirms every shop holds zero transaction references. Pushed so the other
/// shops purge their matching local Item by Id; the server cascade-deletes the
/// SharedItem and its child rows (SharedItemCodes / SharedItemPrices /
/// SharedItemErpMappings / SharedItemDeleteReadiness) when this tombstone applies.
///
/// Propagates through the same channel as <see cref="SharedItemCodeDeletion"/> and
/// participates in the tombstone-ack purge mechanism with TombstoneType "Item".
/// </summary>
public class SharedItemDeletion
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the Item that was deleted. Receivers hard-delete any local Item with
    /// this Id (after the FK safety-net check — see Workstream B).
    /// </summary>
    public Guid DeletedItemId { get; set; }

    public DateTime DeletedAt { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; } = string.Empty;
}
