using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Shared cashier department registry for cross-shop sync.
/// DepartmentNumber is intentionally not stored here — it is the per-shop
/// cashier-button position and must remain shop-local.
/// </summary>
public class SharedCashierDepartment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public Guid VatClassId { get; set; }

    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModifiedByShopId { get; set; }
}
