using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// A supplier's own code (or normalised description) for one of the shared items, as the shops agree on
/// it. A code means the same in every shop, so one matched in one shop saves the others the same work.
///
/// Keyed by the supplier's ΑΦΜ, not a supplier id: transactors are not synced, so the same supplier can
/// have different ids per shop. Only the mapping lives here; times seen and prices stay in each shop.
///
/// There is one row per key, with a version. A push says which version the shop last saw, so the hub can
/// tell a change made knowing the current mapping from one made without it, which is a conflict and is
/// never resolved here. Rows are never deleted: a shop deactivates a code instead.
/// </summary>
public class SharedSupplierItemCode
{
    public int Id { get; set; }

    /// <summary>ΑΦΜ as the comparison key: digits only, no EL/GR prefix, no leading zeros.</summary>
    [Required]
    [MaxLength(20)]
    public string SupplierVat { get; set; }

    /// <summary>1 = the supplier's code, 2 = its normalised description.</summary>
    public int MatchKind { get; set; }

    [Required]
    [MaxLength(300)]
    public string MatchKey { get; set; }

    /// <summary>The shared item. Not a foreign key: a shop may push a code before the item's row settles.</summary>
    public Guid ItemId { get; set; }

    /// <summary>Supplier quantity × factor = quantity in the item's main unit.</summary>
    [Column(TypeName = "decimal(18, 4)")]
    public decimal QuantityFactor { get; set; }

    public bool IsActive { get; set; }

    /// <summary>1 = from history, 2 = manual, 3 = confirmed. A higher value is a stronger decision.</summary>
    public int Source { get; set; }

    /// <summary>The supplier's description, for reading the code in a shop that has not seen it yet.</summary>
    [MaxLength(300)]
    public string SupplierDescription { get; set; }

    /// <summary>Goes up by one each time the mapping changes. Also the concurrency token.</summary>
    public int Version { get; set; }

    /// <summary>When the shop changed the mapping (the shop's clock). Only breaks a tie on the active flag.</summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Server time (UTC) the hub accepted this version. The pull filters on this and not on
    /// <see cref="ModifiedAt"/>, so a code pushed late still reaches a shop that has synced since.
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    [MaxLength(50)]
    public string ModifiedByShopId { get; set; }
}
