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
    // Two-phase delete (Workstream B). Transient cross-shop negotiation state;
    // flows through the normal Items push/pull LWW path like any other field.
    public bool DeleteRequested { get; set; }
    public string? DeleteRequestedByShopId { get; set; }
    public DateTime? DeleteRequestedAt { get; set; }
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

    /// <summary>Earliest known creation of this code across shops (MIN-merged). Null = unknown/legacy.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Latest scan-commit of this code across shops (MAX-merged). Null = never observed.</summary>
    public DateTime? LastUsedAt { get; set; }
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

/// <summary>
/// Tombstone for a deleted ItemCode. Propagates a barcode/supplier-code
/// removal from one shop to the other via the registry. The receiver hard-
/// deletes its matching local row by Id BEFORE its upsert section runs,
/// freeing the (CodeType, Code) unique-index slot for any incoming new row.
/// </summary>
public class SharedItemCodeDeletionDto
{
    public Guid Id { get; set; }
    public Guid DeletedItemCodeId { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

/// <summary>
/// Tombstone for a deleted ItemPriceLevelMapping. Propagates a price-row
/// removal; receiver hard-deletes its matching local row by Id BEFORE the
/// upsert section, freeing the (ItemId, PriceLevelId) unique-index slot.
/// </summary>
public class SharedItemPriceLevelMappingDeletionDto
{
    public Guid Id { get; set; }
    public Guid DeletedMappingId { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

/// <summary>
/// Tombstone for a hard-deleted Item (Workstream B, two-phase delete). Created
/// by the requesting shop at finalize time, after the cross-shop readiness
/// negotiation confirms zero transaction references everywhere. Receiver
/// cascade-deletes the item and its child rows; participates in the tombstone-
/// ack purge mechanism with TombstoneType "Item".
/// </summary>
public class SharedItemDeletionDto
{
    public Guid Id { get; set; }
    public Guid DeletedItemId { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedByShopId { get; set; } = string.Empty;
}

/// <summary>
/// Per-shop average cost carrier. Uploaded during sync (push) and retrieved on
/// demand (GET /api/sharedsync/itemcost/{itemId}) so a shop that sells an item it
/// never purchased can borrow the other shop's cost for COGS / profit analysis.
///
/// Deliberately separate from <see cref="SharedItemDto"/>: AverageCost is NOT part
/// of the item LWW row (a purchase changes AverageCost without bumping
/// Item.ModifiedAt, so it would never push on that channel). UpdatedAt is the LWW
/// key. Mirror any change to the cashier-side GrKoukOrg.Erp.Dtos copy.
/// </summary>
public class SharedItemCostDto
{
    public Guid ItemId { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastPurchasePrice { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ShopId { get; set; } = string.Empty;
}

/// <summary>
/// Per-shop stock snapshot carrier. Uploaded during sync (push) and retrieved on
/// demand (GET /api/sharedsync/stockacrossshops/{itemId}) so one shop can see how
/// much stock a peer holds — a decision aid for inter-shop transfers.
///
/// Separate from <see cref="SharedItemCostDto"/>: that one is gated on a real
/// purchase basis (cost-borrow feature); this carries raw on-hand stock for every
/// item with inventory activity. UpdatedAt is the LWW key. Mirror any change to
/// the cashier-side GrKoukOrg.Erp.Dtos copy.
/// </summary>
public class SharedInventoryDto
{
    public Guid ItemId { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ShopId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for the publishing shop, resolved from the KnownShops
    /// registry (its CompanyCode), falling back to the raw ShopId GUID when the
    /// registry has no name. Mirror to the cashier-side GrKoukOrg.Erp.Dtos copy.
    /// </summary>
    public string ShopName { get; set; } = string.Empty;
}

/// <summary>
/// A shop known to the shared registry (KnownShops row). Returned by
/// GET /api/sharedsync/knownshops so a peer can enumerate the other shops and
/// seed an inter-shop transactor per peer. DisplayName is the peer's CompanyCode
/// (may be null for shops that registered before the name channel existed).
/// </summary>
public class KnownShopDto
{
    public string ShopId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Inter-shop stock transfer header (Feature A transport). Source pushes it after
/// its OUT doc commits; dest pulls it (while Status == "Pushed") and materializes an
/// IN doc, then acks via StockTransferAcks. Immutable once inserted. Mirror to the
/// cashier-side GrKoukOrg.Erp.Dtos copy.
/// </summary>
public class SharedStockTransferDto
{
    public Guid TransferId { get; set; }
    public string SourceShopId { get; set; } = string.Empty;
    public string DestShopId { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Reference { get; set; }
    /// <summary>Set when this transfer ships a stock-request fulfillment. Optional field.</summary>
    public Guid? RequestId { get; set; }
    /// <summary>Paired with RequestId: the claim being shipped — the server flips the matching
    /// fulfillment Claimed → Shipped on push insert. Optional field.</summary>
    public Guid? ClaimId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? MaterializedAt { get; set; }
    public List<SharedStockTransferLineDto> Lines { get; set; } = new();
}

/// <summary>A single item line of a <see cref="SharedStockTransferDto"/>.</summary>
public class SharedStockTransferLineDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    /// <summary>Source item Code, carried for human-readable diagnostics at the dest.</summary>
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal CarriedUnitCost { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Inter-shop stock REQUEST (broadcast request/fulfill transport). The requester pushes it
/// once (insert-immutable header/lines by RequestId); afterwards the ERP row is the single
/// source of truth — per-line Fulfilled/CancelledQuantity and Status are only ever changed by
/// the ERP's atomic claim / release / cancel-remainder endpoints and flow back through the
/// pull: to fulfillers while Status == "Open" (un-since-filtered), to the requester as a
/// since-filtered progress mirror. Mirror to the cashier-side GrKoukOrg.Erp.Dtos copy.
/// </summary>
public class SharedStockRequestDto
{
    public Guid RequestId { get; set; }
    public string RequestingShopId { get; set; } = string.Empty;
    /// <summary>Null = broadcast. Reserved for future targeted requests.</summary>
    public string? TargetShopId { get; set; }
    /// <summary>Future request-kind discriminator; always "Stock" for now.</summary>
    public string RequestType { get; set; } = "Stock";
    public DateTime RequestDate { get; set; }
    public string? Reference { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<SharedStockRequestLineDto> Lines { get; set; } = new();
}

/// <summary>A single item line of a <see cref="SharedStockRequestDto"/>. Remaining is always
/// derived (max(0, Requested − Fulfilled − Cancelled)), never carried.</summary>
public class SharedStockRequestLineDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    /// <summary>Requester item Code, carried for human-readable diagnostics at fulfillers.</summary>
    public string? ItemCode { get; set; }
    public decimal RequestedQuantity { get; set; }
    /// <summary>May exceed RequestedQuantity via confirmed over-fulfillment.</summary>
    public decimal FulfilledQuantity { get; set; }
    public decimal CancelledQuantity { get; set; }
}

/// <summary>
/// Body of the synchronous claim endpoint
/// (POST /api/sharedsync/stockrequests/{requestId}/claim). ClaimId is minted by the caller and
/// makes the call idempotent — replaying a recorded ClaimId returns the original success.
/// FulfillingShopId is verified against the authenticated caller.
/// </summary>
public class StockRequestClaimDto
{
    public Guid ClaimId { get; set; }
    public string FulfillingShopId { get; set; } = string.Empty;
    /// <summary>
    /// True when the operator explicitly confirmed shipping above a line's remaining
    /// (pack/carton rounding). Without it, any over-remaining line fails the claim with 409.
    /// </summary>
    public bool ConfirmedOverFulfill { get; set; }
    public List<StockRequestClaimLineDto> Lines { get; set; } = new();
}

/// <summary>A single line of a <see cref="StockRequestClaimDto"/>.</summary>
public class StockRequestClaimLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Result of the synchronous stock-request endpoints (claim / release / cancel-remainder).
/// Returned with 200 on success and as the 409 body on a lost race — either way
/// <see cref="Request"/> carries the FRESH request state so the caller can refresh in place.
/// </summary>
public class StockRequestOperationResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public SharedStockRequestDto? Request { get; set; }
}

// ─── Request / Response DTOs ────────────────────────────────────────

public class TombstoneAckDto
{
    public Guid TombstoneId { get; set; }
    public string TombstoneType { get; set; } = string.Empty;
}

public class SharedSyncPushRequest
{
    public string ShopId { get; set; } = string.Empty;
    // Human-readable shop label (the pushing shop's CompanyCode from Settings).
    // Stored on KnownShop.DisplayName so peers can show a readable name instead of
    // the raw ShopId GUID (e.g. in the inter-shop transactor / "Άλλα καταστήματα").
    // Optional — old cashier builds send none, server leaves DisplayName untouched.
    public string? CompanyCode { get; set; }
    public List<SharedItemDto> Items { get; set; } = new();
    public List<SharedItemCategoryDto> Categories { get; set; } = new();
    public List<SharedVatClassDto> VatClasses { get; set; } = new();
    public List<SharedMeasureUnitDto> MeasureUnits { get; set; } = new();
    public List<SharedCashierDepartmentDto> CashierDepartments { get; set; } = new();
    public List<SharedItemCodeDto> ItemCodes { get; set; } = new();
    public List<SharedItemPriceDto> ItemPrices { get; set; } = new();
    public List<SharedItemErpMappingDto> ItemErpMappings { get; set; } = new();
    public List<SharedItemErpMappingDeletionDto> ItemErpMappingDeletions { get; set; } = new();
    public List<SharedItemCodeDeletionDto> ItemCodeDeletions { get; set; } = new();
    public List<SharedItemPriceLevelMappingDeletionDto> ItemPriceLevelMappingDeletions { get; set; } = new();
    // Workstream B — Item deletion tombstones. Hard-delete propagation for the
    // two-phase delete flow. Optional field; old builds send none.
    public List<SharedItemDeletionDto> ItemDeletions { get; set; } = new();
    // Phase 3 — Workstream E. Each entry tells the server "shop ShopId has applied this
    // tombstone." Server inserts into SharedTombstoneAcks (UNIQUE-idempotent), then
    // runs a purge pass that hard-deletes tombstones every active KnownShop has acked.
    // Optional field — old cashier builds without this code path send no acks; server
    // treats absent as empty list (backward-compatible).
    public List<TombstoneAckDto> TombstoneAcks { get; set; } = new();
    // Inter-shop cost borrow — per-shop AverageCost for items touched since the last
    // push. Upserted by (ShopId, ItemId), LWW by UpdatedAt. Optional field; old
    // cashier builds send none, old servers ignore it (System.Text.Json).
    public List<SharedItemCostDto> ItemCosts { get; set; } = new();
    // Cross-shop stock query — per-shop on-hand snapshot for items touched since the
    // last push. Upserted by (ShopId, ItemId), LWW by UpdatedAt. Optional field;
    // old cashier builds send none, old servers ignore it (System.Text.Json).
    public List<SharedInventoryDto> Inventories { get; set; } = new();
    // Feature A — inter-shop transfers this shop is publishing (source side). Inserted
    // by TransferId, immutable (skipped if already present). Optional field.
    public List<SharedStockTransferDto> StockTransfers { get; set; } = new();
    // Feature A — TransferIds this shop (dest) has materialized; server flips their
    // Status to "Materialized" and stamps MaterializedAt. Optional field.
    public List<Guid> StockTransferAcks { get; set; } = new();
    // Stock requests this shop authored (requester side). Insert-immutable by RequestId —
    // all later state changes happen ONLY through the synchronous claim/release/
    // cancel-remainder endpoints, never via push. Optional field.
    public List<SharedStockRequestDto> StockRequests { get; set; } = new();
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
    public int ItemCodeDeletionsApplied { get; set; }
    public int ItemPriceLevelMappingDeletionsApplied { get; set; }
    public int ItemDeletionsApplied { get; set; }
    public int TombstoneAcksRecorded { get; set; }
    public int TombstonesPurged { get; set; }
    public int ItemCostsUpserted { get; set; }
    public int InventoriesUpserted { get; set; }
    public int TransfersUpserted { get; set; }
    public int TransferAcksApplied { get; set; }
    public int StockRequestsInserted { get; set; }
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
    public List<SharedItemCodeDeletionDto> ItemCodeDeletions { get; set; } = new();
    public List<SharedItemPriceLevelMappingDeletionDto> ItemPriceLevelMappingDeletions { get; set; } = new();
    public List<SharedItemDeletionDto> ItemDeletions { get; set; } = new();
    // Feature A — inter-shop transfers bound for the requesting shop (dest) that are
    // not yet materialized. Returned regardless of the `since` watermark (the dest
    // must keep receiving them until it acks). Empty for shops with no inbound.
    public List<SharedStockTransferDto> StockTransfers { get; set; } = new();
    // Feature A (step 6) — TransferIds this shop SOURCED that the dest materialized since
    // `since`. Source clears its local Pushed rows so the watchdog stops flagging them.
    public List<Guid> OutboundMaterializedTransferIds { get; set; } = new();
    // Stock requests relevant to this shop, ONE list serving both roles (discriminate by
    // RequestingShopId): foreign rows = OPEN requests this shop could fulfill (returned
    // un-since-filtered so remaining self-refreshes every sync); own rows = progress
    // mirror of my requests whose UpdatedAt > since. Optional field.
    public List<SharedStockRequestDto> StockRequests { get; set; } = new();
    // POSITIVE close signal for fulfillers: RequestIds of FOREIGN requests that left Open
    // (Completed/Cancelled) with UpdatedAt > since. Local open mirrors close ONLY on this,
    // never by absence from StockRequests (absence-inference risks false-clear on a
    // partial pull). Optional field.
    public List<Guid> ClosedStockRequestIds { get; set; } = new();
    public DateTime ServerTimestamp { get; set; }
}
