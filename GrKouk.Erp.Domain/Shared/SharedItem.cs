using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Shared item registry for syncing item definitions between shops.
/// Uses the local GUID as the shared identity (both shops start from the same DB clone).
/// Prices and stock are excluded — they are shop-specific.
/// </summary>
public class SharedItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public bool Active { get; set; }

    // References (GUIDs shared across shops)
    public Guid ItemCategoryId { get; set; }
    public Guid VatClassId { get; set; }
    public Guid MainUnitId { get; set; }
    public Guid? CashierDepartmentId { get; set; }

    // Classification (stored as int for flexibility)
    public int ItemNature { get; set; }
    public int ItemType { get; set; }

    // Various codes
    [MaxLength(50)]
    public string ManufacturerCode { get; set; }
    [MaxLength(50)]
    public string UpcCode { get; set; }
    [MaxLength(50)]
    public string EanCode { get; set; }

    // Costing
    public bool UseBatchTracking { get; set; }

    // Bottle deposit (Εγγύηση Συσκευασίας). DepositItemId points at another
    // SharedItem that represents the packaging deposit charged with this item.
    // No FK constraint here — registry table convention (raw Guid? like
    // CashierDepartmentId); enforcement lives in the cash-register DB.
    public Guid? DepositItemId { get; set; }
    public bool IsDepositItem { get; set; }

    // Tracking
    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; }

    public int Version { get; set; }
}
