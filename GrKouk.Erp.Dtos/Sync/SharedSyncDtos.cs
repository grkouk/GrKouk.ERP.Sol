using System;
using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.Sync;

// ─── Individual entity DTOs ─────────────────────────────────────────

public class SharedItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public Guid ItemCategoryId { get; set; }
    public Guid VatClassId { get; set; }
    public Guid MainUnitId { get; set; }
    public Guid? CashierDepartmentId { get; set; }
    public int ItemNature { get; set; }
    public int ItemType { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? UpcCode { get; set; }
    public string? EanCode { get; set; }
    public bool UseBatchTracking { get; set; }
    public Guid? DepositItemId { get; set; }
    public bool IsDepositItem { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
    public int Version { get; set; }
}

/// <summary>
/// Shared cashier department synced between shops.
/// DepartmentNumber is intentionally excluded — per-shop button mapping.
/// </summary>
public class SharedCashierDepartmentDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid VatClassId { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

public class SharedItemCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
}

public class SharedVatClassDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime ModifiedAt { get; set; }
}

public class SharedMeasureUnitDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
}

public class SharedItemCodeDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int CodeType { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid MeasureUnitId { get; set; }
    public decimal Quantity { get; set; } = 1;
}

public class SharedItemPriceDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid PriceLevelId { get; set; }
    public decimal NetPrice { get; set; }
    public decimal BrutPrice { get; set; }
    public decimal Markup { get; set; }
    public bool IsOverridden { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

public class SharedItemErpMappingDto
{
    public Guid Id { get; set; }
    public Guid LocalItemId { get; set; }
    public int ErpId { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

public class SharedItemErpMappingDeletionDto
{
    public Guid Id { get; set; }
    public Guid LocalItemId { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

// ─── Request / Response DTOs ────────────────────────────────────────

public class SharedSyncPushRequest
{
    public string ShopId { get; set; } = string.Empty;
    public List<SharedItemDto> Items { get; set; } = new();
    public List<SharedItemCategoryDto> Categories { get; set; } = new();
    public List<SharedVatClassDto> VatClasses { get; set; } = new();
    public List<SharedMeasureUnitDto> MeasureUnits { get; set; } = new();
    public List<SharedCashierDepartmentDto> CashierDepartments { get; set; } = new();
    public List<SharedItemCodeDto> ItemCodes { get; set; } = new();
    public List<SharedItemPriceDto> ItemPrices { get; set; } = new();
    public List<SharedItemErpMappingDto> ItemErpMappings { get; set; } = new();
    public List<SharedItemErpMappingDeletionDto> ItemErpMappingDeletions { get; set; } = new();
}

public class SharedSyncPushResponse
{
    public bool Success { get; set; }
    public int ItemsUpserted { get; set; }
    public int CategoriesUpserted { get; set; }
    public int VatClassesUpserted { get; set; }
    public int MeasureUnitsUpserted { get; set; }
    public int CashierDepartmentsUpserted { get; set; }
    public int ItemCodesUpserted { get; set; }
    public int ItemPricesUpserted { get; set; }
    public int ItemErpMappingsUpserted { get; set; }
    public int ItemErpMappingDeletionsApplied { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class SharedSyncPullResponse
{
    public List<SharedItemDto> Items { get; set; } = new();
    public List<SharedItemCategoryDto> Categories { get; set; } = new();
    public List<SharedVatClassDto> VatClasses { get; set; } = new();
    public List<SharedMeasureUnitDto> MeasureUnits { get; set; } = new();
    public List<SharedCashierDepartmentDto> CashierDepartments { get; set; } = new();
    public List<SharedItemCodeDto> ItemCodes { get; set; } = new();
    public List<SharedItemPriceDto> ItemPrices { get; set; } = new();
    public List<SharedItemErpMappingDto> ItemErpMappings { get; set; } = new();
    public List<SharedItemErpMappingDeletionDto> ItemErpMappingDeletions { get; set; } = new();
    public DateTime ServerTimestamp { get; set; }
}
