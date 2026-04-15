using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Shared ItemErpMapping row replicated across shops. Each row links a local
/// Item (by LocalItemId GUID, identical across cloned shop databases) to the
/// ERP financial aggregate it resolves to (ErpId, an int FK into the ERP DB).
/// One alive mapping per LocalItemId is enforced via unique index.
/// </summary>
public class SharedItemErpMapping
{
    [Key]
    public Guid Id { get; set; }

    public Guid LocalItemId { get; set; }

    public int ErpId { get; set; }

    public DateTime LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; } = string.Empty;
}
