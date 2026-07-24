using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Sync;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Controllers;

/// <summary>
/// Handles cross-shop item synchronization via a shared registry.
/// Both shops share the same GUIDs (shop 2 was cloned from shop 1).
/// Syncs items, reference data, and inter-shop price level prices.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SharedSyncController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<SharedSyncController> _logger;

    public SharedSyncController(ApiDbContext context, ILogger<SharedSyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Pull changes from the shared registry.
    /// Returns items and reference data modified since the given timestamp,
    /// excluding changes that originated from the requesting shop.
    /// </summary>
    [HttpGet("pull")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> Pull([FromQuery] DateTime since, [FromQuery] string shopId)
    {
        if (string.IsNullOrWhiteSpace(shopId))
            return BadRequest(new { error = "shopId is required" });

        var serverTimestamp = DateTime.UtcNow;

        var items = await _context.SharedItems
            .Where(i => i.ModifiedAt > since && i.ModifiedByShopId != shopId)
            .Select(i => new SharedItemDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                Active = i.Active,
                ItemCategoryId = i.ItemCategoryId,
                VatClassId = i.VatClassId,
                MainUnitId = i.MainUnitId,
                CashierDepartmentId = i.CashierDepartmentId,
                ItemNature = i.ItemNature,
                ItemType = i.ItemType,
                ManufacturerCode = i.ManufacturerCode,
                UpcCode = i.UpcCode,
                EanCode = i.EanCode,
                UseBatchTracking = i.UseBatchTracking,
                DepositItemId = i.DepositItemId,
                IsDepositItem = i.IsDepositItem,
                DeleteRequested = i.DeleteRequested,
                DeleteRequestedByShopId = i.DeleteRequestedByShopId,
                DeleteRequestedAt = i.DeleteRequestedAt,
                ModifiedAt = i.ModifiedAt,
                ModifiedByShopId = i.ModifiedByShopId,
                Version = i.Version
            })
            .ToListAsync();

        var categories = await _context.SharedItemCategories
            .Where(c => c.ModifiedAt > since)
            .Select(c => new SharedItemCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ModifiedAt = c.ModifiedAt
            })
            .ToListAsync();

        var vatClasses = await _context.SharedVatClasses
            .Where(v => v.ModifiedAt > since)
            .Select(v => new SharedVatClassDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Rate = v.Rate,
                ModifiedAt = v.ModifiedAt
            })
            .ToListAsync();

        var measureUnits = await _context.SharedMeasureUnits
            .Where(m => m.ModifiedAt > since)
            .Select(m => new SharedMeasureUnitDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                ModifiedAt = m.ModifiedAt
            })
            .ToListAsync();

        var cashierDepartments = await _context.SharedCashierDepartments
            .Where(d => d.ModifiedAt > since && d.ModifiedByShopId != shopId)
            .Select(d => new SharedCashierDepartmentDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                VatClassId = d.VatClassId,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemPrices = await _context.SharedItemPrices
            .Where(p => p.ModifiedAt > since && p.ModifiedByShopId != shopId)
            .Select(p => new SharedItemPriceDto
            {
                Id = p.Id,
                ItemId = p.ItemId,
                PriceLevelId = p.PriceLevelId,
                NetPrice = p.NetPrice,
                BrutPrice = p.BrutPrice,
                Markup = p.Markup,
                IsOverridden = p.IsOverridden,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                IsActive = p.IsActive,
                ModifiedAt = p.ModifiedAt,
                ModifiedByShopId = p.ModifiedByShopId
            })
            .ToListAsync();

        var itemErpMappings = await _context.SharedItemErpMappings
            .Where(m => m.ModifiedAt > since && m.ModifiedByShopId != shopId)
            .Select(m => new SharedItemErpMappingDto
            {
                Id = m.Id,
                LocalItemId = m.LocalItemId,
                ErpId = m.ErpId,
                LastSyncedAt = m.LastSyncedAt,
                CreatedAt = m.CreatedAt,
                ModifiedAt = m.ModifiedAt,
                ModifiedByShopId = m.ModifiedByShopId
            })
            .ToListAsync();

        // Mappings/prices can be tagged ModifiedByShopId=B while their parent Item is
        // tagged ModifiedByShopId=A (different push events). The echo filter on items
        // (ModifiedByShopId != shopId) then drops the parent for shop A's pull, leaving
        // the receiver with FK_ItemErpMappings_Items_LocalItemId. Pull in any missing
        // FK parents now, bypassing the echo filter — they're for FK satisfaction, not
        // change notification. The receiver's LWW guard skips redundant updates.
        // Stale-barcode feature: SharedItemCode.LastUsedAt is its own pull-out
        // watermark. A usage stamp changes no other column and never bumps the
        // parent SharedItem.ModifiedAt, so codes selected only via parent-item
        // recency would never carry stamps to the peer shop and the cross-shop
        // MAX-merge would never converge. Their parent items also join the
        // FK-parent pull-in below. (No ModifiedByShopId on codes → the pushing
        // shop gets its own stamp echoed back; the client merge is idempotent.)
        var stampedCodeItemIds = await _context.SharedItemCodes
            .Where(ic => ic.LastUsedAt > since)
            .Select(ic => ic.ItemId)
            .Distinct()
            .ToListAsync();

        var includedItemIds = items.Select(i => i.Id).ToHashSet();
        var referencedItemIds = itemErpMappings.Select(m => m.LocalItemId)
            .Concat(itemPrices.Select(p => p.ItemId))
            .Concat(stampedCodeItemIds)
            .Where(id => !includedItemIds.Contains(id))
            .Distinct()
            .ToList();
        if (referencedItemIds.Count > 0)
        {
            var extraItems = await _context.SharedItems
                .Where(i => referencedItemIds.Contains(i.Id))
                .Select(i => new SharedItemDto
                {
                    Id = i.Id,
                    Code = i.Code,
                    Name = i.Name,
                    Active = i.Active,
                    ItemCategoryId = i.ItemCategoryId,
                    VatClassId = i.VatClassId,
                    MainUnitId = i.MainUnitId,
                    ItemNature = i.ItemNature,
                    ItemType = i.ItemType,
                    ManufacturerCode = i.ManufacturerCode,
                    UpcCode = i.UpcCode,
                    EanCode = i.EanCode,
                    CashierDepartmentId = i.CashierDepartmentId,
                    UseBatchTracking = i.UseBatchTracking,
                    DepositItemId = i.DepositItemId,
                    IsDepositItem = i.IsDepositItem,
                    DeleteRequested = i.DeleteRequested,
                    DeleteRequestedByShopId = i.DeleteRequestedByShopId,
                    DeleteRequestedAt = i.DeleteRequestedAt,
                    ModifiedAt = i.ModifiedAt,
                    ModifiedByShopId = i.ModifiedByShopId,
                    Version = i.Version
                })
                .ToListAsync();
            items.AddRange(extraItems);
            _logger.LogInformation(
                "SharedSync Pull for shop {ShopId}: pulled in {Found}/{Requested} FK parent items bypassing echo filter",
                shopId, extraItems.Count, referencedItemIds.Count);
        }

        // Items have FKs to ItemCategories, VatClasses, MeasureUnits. The ref-data queries
        // above filter by ModifiedAt > since, so an item that pulls in a FK-parent ref row
        // would FK-fail on the receiver if that ref row was created pre-`since`. Fill in
        // the missing ref rows now, bypassing the time filter (FK satisfaction, not change
        // notification).
        var includedCategoryIds = categories.Select(c => c.Id).ToHashSet();
        var referencedCategoryIds = items.Select(i => i.ItemCategoryId)
            .Where(id => !includedCategoryIds.Contains(id))
            .Distinct()
            .ToList();
        if (referencedCategoryIds.Count > 0)
        {
            var extraCategories = await _context.SharedItemCategories
                .Where(c => referencedCategoryIds.Contains(c.Id))
                .Select(c => new SharedItemCategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    ModifiedAt = c.ModifiedAt
                })
                .ToListAsync();
            categories.AddRange(extraCategories);
        }

        var includedVatClassIds = vatClasses.Select(v => v.Id).ToHashSet();
        var referencedVatClassIds = items.Select(i => i.VatClassId)
            .Where(id => !includedVatClassIds.Contains(id))
            .Distinct()
            .ToList();
        if (referencedVatClassIds.Count > 0)
        {
            var extraVatClasses = await _context.SharedVatClasses
                .Where(v => referencedVatClassIds.Contains(v.Id))
                .Select(v => new SharedVatClassDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Rate = v.Rate,
                    ModifiedAt = v.ModifiedAt
                })
                .ToListAsync();
            vatClasses.AddRange(extraVatClasses);
        }

        var includedMeasureUnitIds = measureUnits.Select(m => m.Id).ToHashSet();
        var referencedMeasureUnitIds = items.Select(i => i.MainUnitId)
            .Where(id => !includedMeasureUnitIds.Contains(id))
            .Distinct()
            .ToList();
        if (referencedMeasureUnitIds.Count > 0)
        {
            var extraMeasureUnits = await _context.SharedMeasureUnits
                .Where(m => referencedMeasureUnitIds.Contains(m.Id))
                .Select(m => new SharedMeasureUnitDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    ModifiedAt = m.ModifiedAt
                })
                .ToListAsync();
            measureUnits.AddRange(extraMeasureUnits);
        }

        var includedDeptIds = cashierDepartments.Select(d => d.Id).ToHashSet();
        var referencedDeptIds = items
            .Where(i => i.CashierDepartmentId.HasValue)
            .Select(i => i.CashierDepartmentId!.Value)
            .Where(id => !includedDeptIds.Contains(id))
            .Distinct()
            .ToList();
        if (referencedDeptIds.Count > 0)
        {
            var extraDepts = await _context.SharedCashierDepartments
                .Where(d => referencedDeptIds.Contains(d.Id))
                .Select(d => new SharedCashierDepartmentDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.Name,
                    VatClassId = d.VatClassId,
                    ModifiedAt = d.ModifiedAt,
                    ModifiedByShopId = d.ModifiedByShopId
                })
                .ToListAsync();
            cashierDepartments.AddRange(extraDepts);
        }

        // For item codes, return codes belonging to items in the (now FK-complete)
        // batch, plus usage-stamped codes riding their own LastUsedAt watermark
        // (their parents are already in the batch via stampedCodeItemIds above;
        // the explicit clause keeps the watermark independent of that pull-in).
        var allItemIds = items.Select(i => i.Id).ToHashSet();
        var itemCodes = await _context.SharedItemCodes
            .Where(ic => allItemIds.Contains(ic.ItemId) || ic.LastUsedAt > since)
            .Select(ic => new SharedItemCodeDto
            {
                Id = ic.Id,
                ItemId = ic.ItemId,
                CodeType = ic.CodeType,
                Code = ic.Code,
                MeasureUnitId = ic.MeasureUnitId,
                Quantity = ic.Quantity,
                CreatedAt = ic.CreatedAt,
                LastUsedAt = ic.LastUsedAt
            })
            .ToListAsync();

        var itemErpMappingDeletions = await _context.SharedItemErpMappingDeletions
            .Where(d => d.ModifiedAt > since && d.ModifiedByShopId != shopId)
            .Select(d => new SharedItemErpMappingDeletionDto
            {
                Id = d.Id,
                LocalItemId = d.LocalItemId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemCodeDeletions = await _context.SharedItemCodeDeletions
            .Where(d => d.ModifiedAt > since && d.ModifiedByShopId != shopId)
            .Select(d => new SharedItemCodeDeletionDto
            {
                Id = d.Id,
                DeletedItemCodeId = d.DeletedItemCodeId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemPriceLevelMappingDeletions = await _context.SharedItemPriceLevelMappingDeletions
            .Where(d => d.ModifiedAt > since && d.ModifiedByShopId != shopId)
            .Select(d => new SharedItemPriceLevelMappingDeletionDto
            {
                Id = d.Id,
                DeletedMappingId = d.DeletedMappingId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemDeletions = await _context.SharedItemDeletions
            .Where(d => d.ModifiedAt > since && d.ModifiedByShopId != shopId)
            .Select(d => new SharedItemDeletionDto
            {
                Id = d.Id,
                DeletedItemId = d.DeletedItemId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        // Feature A — inbound transfers for this shop. NOT filtered by `since`: a dest
        // must keep receiving a transfer on every pull until it materializes + acks it
        // (Status flips to Materialized), so it can recover from a missed apply.
        var stockTransfers = await _context.SharedStockTransfers
            .Where(t => t.DestShopId == shopId && t.Status == StockTransferStatuses.Pushed)
            .Select(t => new SharedStockTransferDto
            {
                TransferId = t.TransferId,
                SourceShopId = t.SourceShopId,
                DestShopId = t.DestShopId,
                TransactionDate = t.TransactionDate,
                Reference = t.Reference,
                Status = t.Status,
                MaterializedAt = t.MaterializedAt,
                Lines = t.Lines.Select(l => new SharedStockTransferLineDto
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    ItemCode = l.ItemCode,
                    Quantity = l.Quantity,
                    CarriedUnitCost = l.CarriedUnitCost,
                    BatchNumber = l.BatchNumber,
                    ExpiryDate = l.ExpiryDate
                }).ToList()
            })
            .ToListAsync();

        // Feature A (step 6) — positive materialization confirmations for transfers THIS
        // shop SOURCED, so it can clear its local Pushed rows and stop the watchdog from
        // flagging them. Bounded by `since` (only newly materialized) — a missed update
        // just leaves the source's row Pushed (false-pending, the SAFE direction; never a
        // false-clear, which absence-based inference would risk).
        var outboundMaterializedTransferIds = await _context.SharedStockTransfers
            .Where(t => t.SourceShopId == shopId
                        && t.Status == StockTransferStatuses.Materialized
                        && t.MaterializedAt != null
                        && t.MaterializedAt > since)
            .Select(t => t.TransferId)
            .ToListAsync();

        // Stock requests, ONE list serving both roles (client discriminates by
        // RequestingShopId):
        //  - FOREIGN OPEN requests this shop could fulfill. NOT filtered by `since` —
        //    the fulfiller keeps receiving them until they close, so per-line
        //    Fulfilled/CancelledQuantity self-refreshes every sync.
        //  - The shop's OWN requests whose UpdatedAt moved past `since` — the progress
        //    mirror (claims/cancel/completion made at the ERP flow back to the requester).
        var stockRequests = await _context.SharedStockRequests
            .Where(r => (r.RequestingShopId != shopId
                         && r.Status == StockRequestStatuses.Open
                         && (r.TargetShopId == null || r.TargetShopId == shopId))
                        || (r.RequestingShopId == shopId && r.UpdatedAt > since))
            .Select(r => new SharedStockRequestDto
            {
                RequestId = r.RequestId,
                RequestingShopId = r.RequestingShopId,
                TargetShopId = r.TargetShopId,
                RequestType = r.RequestType,
                RequestDate = r.RequestDate,
                Reference = r.Reference,
                Status = r.Status,
                UpdatedAt = r.UpdatedAt,
                CompletedAt = r.CompletedAt,
                Lines = r.Lines.Select(l => new SharedStockRequestLineDto
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    ItemCode = l.ItemCode,
                    RequestedQuantity = l.RequestedQuantity,
                    FulfilledQuantity = l.FulfilledQuantity,
                    CancelledQuantity = l.CancelledQuantity
                }).ToList()
            })
            .ToListAsync();

        // POSITIVE close signal for fulfillers: FOREIGN requests that left Open since
        // the watermark. Local open mirrors close ONLY on this — never by absence from
        // the open list above, which would false-clear on a partial pull (same principle
        // as OutboundMaterializedTransferIds). A missed close just leaves a stale grid
        // row whose claim gets 409 — the SAFE direction.
        var closedStockRequestIds = await _context.SharedStockRequests
            .Where(r => r.RequestingShopId != shopId
                        && r.Status != StockRequestStatuses.Open
                        && (r.TargetShopId == null || r.TargetShopId == shopId)
                        && r.UpdatedAt > since)
            .Select(r => r.RequestId)
            .ToListAsync();

        _logger.LogInformation(
            "SharedSync Pull for shop {ShopId} since {Since}: {Items} items, {Categories} categories, {VatClasses} vat classes, {MeasureUnits} measure units, {Depts} cashier depts, {ItemCodes} item codes ({IcDel} deletes), {ItemPrices} item prices ({IpDel} deletes), {Mappings} mappings, {MapDeletions} mapping deletions",
            shopId, since, items.Count, categories.Count, vatClasses.Count, measureUnits.Count, cashierDepartments.Count, itemCodes.Count, itemCodeDeletions.Count, itemPrices.Count, itemPriceLevelMappingDeletions.Count, itemErpMappings.Count, itemErpMappingDeletions.Count);

        return Ok(new SharedSyncPullResponse
        {
            Items = items,
            Categories = categories,
            VatClasses = vatClasses,
            MeasureUnits = measureUnits,
            CashierDepartments = cashierDepartments,
            ItemCodes = itemCodes,
            ItemPrices = itemPrices,
            ItemErpMappings = itemErpMappings,
            ItemErpMappingDeletions = itemErpMappingDeletions,
            ItemCodeDeletions = itemCodeDeletions,
            ItemPriceLevelMappingDeletions = itemPriceLevelMappingDeletions,
            ItemDeletions = itemDeletions,
            StockTransfers = stockTransfers,
            OutboundMaterializedTransferIds = outboundMaterializedTransferIds,
            StockRequests = stockRequests,
            ClosedStockRequestIds = closedStockRequestIds,
            ServerTimestamp = serverTimestamp
        });
    }

    /// <summary>
    /// Push local changes to the shared registry.
    /// Upserts all records by GUID. Uses ModifiedAt for last-write-wins conflict resolution.
    /// </summary>
    [HttpPost("push")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> Push([FromBody] SharedSyncPushRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Empty request" });

        if (string.IsNullOrWhiteSpace(request.ShopId))
            return BadRequest(new { error = "ShopId is required" });

        var errors = new List<string>();
        int categoriesUpserted = 0;
        int vatClassesUpserted = 0;
        int measureUnitsUpserted = 0;
        int cashierDepartmentsUpserted = 0;
        int itemsUpserted = 0;
        int itemCodesUpserted = 0;
        int itemPricesUpserted = 0;
        int itemErpMappingsUpserted = 0;
        int itemErpMappingDeletionsApplied = 0;
        int itemCodeDeletionsApplied = 0;
        int itemPriceLevelMappingDeletionsApplied = 0;
        int itemDeletionsApplied = 0;
        int tombstoneAcksRecorded = 0;
        int tombstonesPurged = 0;
        int itemCostsUpserted = 0;
        int inventoriesUpserted = 0;
        int transfersUpserted = 0;
        int transferAcksApplied = 0;
        int stockRequestsInserted = 0;

        try
        {
            // 0. Auto-register / heartbeat the calling shop in KnownShops. This is the
            // anchor used by the tombstone-purge pass (a tombstone is purgeable once
            // every active KnownShop has acked it). Bumping LastSeenAt on every push
            // lets an admin spot stale shops via SELECT * FROM KnownShops ORDER BY LastSeenAt.
            var knownShop = await _context.KnownShops.FindAsync(request.ShopId);
            if (knownShop == null)
            {
                _context.KnownShops.Add(new KnownShop
                {
                    ShopId = request.ShopId,
                    DisplayName = string.IsNullOrWhiteSpace(request.CompanyCode) ? null : request.CompanyCode,
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
            else
            {
                knownShop.LastSeenAt = DateTime.UtcNow;
                // Refresh the display label from the caller's CompanyCode when supplied
                // (keeps it current if the shop renamed; never blanks an existing name).
                if (!string.IsNullOrWhiteSpace(request.CompanyCode))
                    knownShop.DisplayName = request.CompanyCode;
            }

            // 1. Reference data first (items depend on these)
            foreach (var cat in request.Categories)
            {
                var existing = await _context.SharedItemCategories.FindAsync(cat.Id);
                if (existing == null)
                {
                    _context.SharedItemCategories.Add(new SharedItemCategory
                    {
                        Id = cat.Id,
                        Code = cat.Code,
                        Name = cat.Name,
                        ModifiedAt = cat.ModifiedAt
                    });
                    categoriesUpserted++;
                }
                else if (cat.ModifiedAt > existing.ModifiedAt)
                {
                    existing.Code = cat.Code;
                    existing.Name = cat.Name;
                    existing.ModifiedAt = cat.ModifiedAt;
                    categoriesUpserted++;
                }
            }

            foreach (var vc in request.VatClasses)
            {
                var existing = await _context.SharedVatClasses.FindAsync(vc.Id);
                if (existing == null)
                {
                    _context.SharedVatClasses.Add(new SharedVatClass
                    {
                        Id = vc.Id,
                        Code = vc.Code,
                        Name = vc.Name,
                        Rate = vc.Rate,
                        ModifiedAt = vc.ModifiedAt
                    });
                    vatClassesUpserted++;
                }
                else if (vc.ModifiedAt > existing.ModifiedAt)
                {
                    existing.Code = vc.Code;
                    existing.Name = vc.Name;
                    existing.Rate = vc.Rate;
                    existing.ModifiedAt = vc.ModifiedAt;
                    vatClassesUpserted++;
                }
            }

            foreach (var mu in request.MeasureUnits)
            {
                var existing = await _context.SharedMeasureUnits.FindAsync(mu.Id);
                if (existing == null)
                {
                    _context.SharedMeasureUnits.Add(new SharedMeasureUnit
                    {
                        Id = mu.Id,
                        Code = mu.Code,
                        Name = mu.Name,
                        ModifiedAt = mu.ModifiedAt
                    });
                    measureUnitsUpserted++;
                }
                else if (mu.ModifiedAt > existing.ModifiedAt)
                {
                    existing.Code = mu.Code;
                    existing.Name = mu.Name;
                    existing.ModifiedAt = mu.ModifiedAt;
                    measureUnitsUpserted++;
                }
            }

            // 1d. Cashier departments — DepartmentNumber is intentionally not in the
            // wire format; the registry never stores it. Each shop owns its own
            // button-to-department mapping locally.
            foreach (var cd in request.CashierDepartments)
            {
                var existing = await _context.SharedCashierDepartments.FindAsync(cd.Id);
                if (existing == null)
                {
                    _context.SharedCashierDepartments.Add(new SharedCashierDepartment
                    {
                        Id = cd.Id,
                        Code = cd.Code,
                        Name = cd.Name,
                        VatClassId = cd.VatClassId,
                        ModifiedAt = cd.ModifiedAt,
                        ModifiedByShopId = request.ShopId
                    });
                    cashierDepartmentsUpserted++;
                }
                else if (cd.ModifiedAt > existing.ModifiedAt)
                {
                    existing.Code = cd.Code;
                    existing.Name = cd.Name;
                    existing.VatClassId = cd.VatClassId;
                    existing.ModifiedAt = cd.ModifiedAt;
                    existing.ModifiedByShopId = request.ShopId;
                    cashierDepartmentsUpserted++;
                }
            }

            // 2. Items
            foreach (var item in request.Items)
            {
                var existing = await _context.SharedItems.FindAsync(item.Id);
                if (existing == null)
                {
                    _context.SharedItems.Add(new SharedItem
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        Active = item.Active,
                        ItemCategoryId = item.ItemCategoryId,
                        VatClassId = item.VatClassId,
                        MainUnitId = item.MainUnitId,
                        CashierDepartmentId = item.CashierDepartmentId,
                        ItemNature = item.ItemNature,
                        ItemType = item.ItemType,
                        ManufacturerCode = item.ManufacturerCode,
                        UpcCode = item.UpcCode,
                        EanCode = item.EanCode,
                        UseBatchTracking = item.UseBatchTracking,
                        DepositItemId = item.DepositItemId,
                        IsDepositItem = item.IsDepositItem,
                        DeleteRequested = item.DeleteRequested,
                        DeleteRequestedByShopId = item.DeleteRequestedByShopId,
                        DeleteRequestedAt = item.DeleteRequestedAt,
                        ModifiedAt = item.ModifiedAt,
                        ModifiedByShopId = request.ShopId,
                        Version = 1
                    });
                    itemsUpserted++;
                }
                else if (item.ModifiedAt > existing.ModifiedAt)
                {
                    existing.Code = item.Code;
                    existing.Name = item.Name;
                    existing.Active = item.Active;
                    existing.ItemCategoryId = item.ItemCategoryId;
                    existing.VatClassId = item.VatClassId;
                    existing.MainUnitId = item.MainUnitId;
                    existing.CashierDepartmentId = item.CashierDepartmentId;
                    existing.ItemNature = item.ItemNature;
                    existing.ItemType = item.ItemType;
                    existing.ManufacturerCode = item.ManufacturerCode;
                    existing.UpcCode = item.UpcCode;
                    existing.EanCode = item.EanCode;
                    existing.UseBatchTracking = item.UseBatchTracking;
                    existing.DepositItemId = item.DepositItemId;
                    existing.IsDepositItem = item.IsDepositItem;
                    existing.DeleteRequested = item.DeleteRequested;
                    existing.DeleteRequestedByShopId = item.DeleteRequestedByShopId;
                    existing.DeleteRequestedAt = item.DeleteRequestedAt;
                    existing.ModifiedAt = item.ModifiedAt;
                    existing.ModifiedByShopId = request.ShopId;
                    existing.Version++;
                    itemsUpserted++;
                }
            }

            // 2b. ItemCode tombstones — apply BEFORE the ItemCodes section below.
            // The receiving shop applies its incoming tombstones before upserts in
            // its own pull, so the wire payload from THIS push should be ordered
            // analogously: tombstones land in the registry first, then the live
            // SharedItemCodes get re-shaped by the items section.
            //
            // Each incoming tombstone is recorded in SharedItemCodeDeletions (idempotent
            // by Id) AND the matching live SharedItemCodes row is removed, so a
            // subsequent pull from the OTHER shop sees a clean (no live row, plus a
            // tombstone) state.
            foreach (var del in request.ItemCodeDeletions)
            {
                var existingDel = await _context.SharedItemCodeDeletions.FindAsync(del.Id);
                if (existingDel == null)
                {
                    _context.SharedItemCodeDeletions.Add(new SharedItemCodeDeletion
                    {
                        Id = del.Id,
                        DeletedItemCodeId = del.DeletedItemCodeId,
                        DeletedAt = del.DeletedAt,
                        // Always stamp ModifiedAt with server-clock UtcNow so the
                        // pull-time filter (ModifiedAt > since) works regardless of
                        // what the client sent. Clients may omit the field; relying
                        // on their wall-clock would also expose us to drift.
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedByShopId = request.ShopId
                    });
                    itemCodeDeletionsApplied++;

                    // Auto-ack the originating shop. They created and pushed this tombstone,
                    // which means they have already applied it locally. Saves us from touching
                    // every cashier-side local-delete site to push pending-ack rows.
                    _context.SharedTombstoneAcks.Add(new SharedTombstoneAck
                    {
                        Id = Guid.NewGuid(),
                        TombstoneId = del.Id,
                        TombstoneType = TombstoneTypes.ItemCode,
                        ShopId = request.ShopId,
                        AckedAt = DateTime.UtcNow
                    });
                    tombstoneAcksRecorded++;
                }

                // Drop the live registry row this tombstone targets — by Id, since
                // the cash-register shops share GUIDs.
                var liveCode = await _context.SharedItemCodes
                    .FirstOrDefaultAsync(ic => ic.Id == del.DeletedItemCodeId);
                if (liveCode != null)
                {
                    _context.SharedItemCodes.Remove(liveCode);
                }
            }

            // 3. Item codes — per-row upsert by Id. Deletions go through tombstones (see 2b).
            //
            // Was replace-all (RemoveRange + re-add) until 2026-05-19. That implicitly
            // deleted any server-side row a client didn't include in its push payload,
            // which silently wiped barcodes whenever the client's local view of an
            // item's ItemCodes was incomplete (e.g. one barcode skipped on apply due to
            // natural-key collision, then echoed back as part of the same SyncAsync's
            // push). See project_sync_phase3_and_item_deletion.
            //
            // The tombstone handler at 2b above already removes rows the client
            // explicitly tombstoned. Anything else stays.
            foreach (var ic in request.ItemCodes)
            {
                var existing = await _context.SharedItemCodes.FindAsync(ic.Id);
                if (existing == null)
                {
                    _context.SharedItemCodes.Add(new SharedItemCode
                    {
                        Id = ic.Id,
                        ItemId = ic.ItemId,
                        CodeType = ic.CodeType,
                        Code = ic.Code,
                        MeasureUnitId = ic.MeasureUnitId,
                        Quantity = ic.Quantity,
                        CreatedAt = ic.CreatedAt,
                        LastUsedAt = ic.LastUsedAt
                    });
                }
                else
                {
                    existing.ItemId = ic.ItemId;
                    existing.CodeType = ic.CodeType;
                    existing.Code = ic.Code;
                    existing.MeasureUnitId = ic.MeasureUnitId;
                    existing.Quantity = ic.Quantity;
                    // Stale-barcode feature: convergent merge, NOT last-writer-wins.
                    // Both shops push usage stamps independently; verbatim assignment
                    // would let one shop's older stamp overwrite the other's newer one.
                    // The server is the convergence hub: keep the LATEST LastUsedAt
                    // (a code is stale only when stale in BOTH shops) and the EARLIEST
                    // CreatedAt (null = unknown/legacy never overwrites a known value).
                    if (ic.LastUsedAt.HasValue &&
                        (existing.LastUsedAt == null || ic.LastUsedAt > existing.LastUsedAt))
                    {
                        existing.LastUsedAt = ic.LastUsedAt;
                    }
                    if (ic.CreatedAt.HasValue &&
                        (existing.CreatedAt == null || ic.CreatedAt < existing.CreatedAt))
                    {
                        existing.CreatedAt = ic.CreatedAt;
                    }
                }
                itemCodesUpserted++;
            }

            // 3b. ItemPriceLevelMapping tombstones — same shape as ItemCode tombstones.
            // No UI path on the cash register produces these today, but the channel is
            // open so the other shop can start producing them once its Phase 1 ships.
            foreach (var del in request.ItemPriceLevelMappingDeletions)
            {
                var existingDel = await _context.SharedItemPriceLevelMappingDeletions.FindAsync(del.Id);
                if (existingDel == null)
                {
                    _context.SharedItemPriceLevelMappingDeletions.Add(new SharedItemPriceLevelMappingDeletion
                    {
                        Id = del.Id,
                        DeletedMappingId = del.DeletedMappingId,
                        DeletedAt = del.DeletedAt,
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedByShopId = request.ShopId
                    });
                    itemPriceLevelMappingDeletionsApplied++;

                    _context.SharedTombstoneAcks.Add(new SharedTombstoneAck
                    {
                        Id = Guid.NewGuid(),
                        TombstoneId = del.Id,
                        TombstoneType = TombstoneTypes.ItemPriceLevelMapping,
                        ShopId = request.ShopId,
                        AckedAt = DateTime.UtcNow
                    });
                    tombstoneAcksRecorded++;
                }

                var livePrice = await _context.SharedItemPrices
                    .FirstOrDefaultAsync(p => p.Id == del.DeletedMappingId);
                if (livePrice != null)
                {
                    _context.SharedItemPrices.Remove(livePrice);
                }
            }

            // 4. Item prices — upsert with last-write-wins
            foreach (var price in request.ItemPrices)
            {
                var existing = await _context.SharedItemPrices.FindAsync(price.Id);
                if (existing == null)
                {
                    _context.SharedItemPrices.Add(new SharedItemPrice
                    {
                        Id = price.Id,
                        ItemId = price.ItemId,
                        PriceLevelId = price.PriceLevelId,
                        NetPrice = price.NetPrice,
                        BrutPrice = price.BrutPrice,
                        Markup = price.Markup,
                        IsOverridden = price.IsOverridden,
                        ValidFrom = price.ValidFrom,
                        ValidTo = price.ValidTo,
                        IsActive = price.IsActive,
                        ModifiedAt = price.ModifiedAt,
                        ModifiedByShopId = request.ShopId
                    });
                    itemPricesUpserted++;
                }
                else if (price.ModifiedAt > existing.ModifiedAt)
                {
                    existing.NetPrice = price.NetPrice;
                    existing.BrutPrice = price.BrutPrice;
                    existing.Markup = price.Markup;
                    existing.IsOverridden = price.IsOverridden;
                    existing.ValidFrom = price.ValidFrom;
                    existing.ValidTo = price.ValidTo;
                    existing.IsActive = price.IsActive;
                    existing.ModifiedAt = price.ModifiedAt;
                    existing.ModifiedByShopId = request.ShopId;
                    itemPricesUpserted++;
                }
            }

            // 5. ItemErpMapping deletions — apply tombstones first so a same-batch
            // re-map (delete old, create new for same LocalItemId) resolves correctly.
            //
            // Bug-fix 2026-05-15: stamp ModifiedAt with server-clock UtcNow on
            // receipt and use del.DeletedAt (not del.ModifiedAt) for the alive-mapping
            // freshness compare. The cash-register's SharedItemErpMappingDeletionDto
            // never sets ModifiedAt — JSON-wise the field arrives as default(DateTime)
            // = 0001-01-01. Storing that meant the Pull filter `ModifiedAt > since`
            // never matched (tombstones invisible to the other shop) AND the
            // alive-mapping comparison `aliveMapping.ModifiedAt < del.ModifiedAt` was
            // never true (live mapping never removed). Same shape as the new
            // ItemCode/PriceLevel tombstone handlers above.
            foreach (var del in request.ItemErpMappingDeletions)
            {
                var existingDel = await _context.SharedItemErpMappingDeletions.FindAsync(del.Id);
                if (existingDel == null)
                {
                    _context.SharedItemErpMappingDeletions.Add(new SharedItemErpMappingDeletion
                    {
                        Id = del.Id,
                        LocalItemId = del.LocalItemId,
                        DeletedAt = del.DeletedAt,
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedByShopId = request.ShopId
                    });
                    itemErpMappingDeletionsApplied++;

                    _context.SharedTombstoneAcks.Add(new SharedTombstoneAck
                    {
                        Id = Guid.NewGuid(),
                        TombstoneId = del.Id,
                        TombstoneType = TombstoneTypes.ItemErpMapping,
                        ShopId = request.ShopId,
                        AckedAt = DateTime.UtcNow
                    });
                    tombstoneAcksRecorded++;
                }

                // Remove the alive mapping for this LocalItemId if it was modified
                // before the deletion happened — otherwise another shop has already
                // re-mapped past this tombstone and we leave it alone. Compare against
                // del.DeletedAt because that is what the client actually populates;
                // del.ModifiedAt is unset on the wire.
                var aliveMapping = await _context.SharedItemErpMappings
                    .FirstOrDefaultAsync(m => m.LocalItemId == del.LocalItemId);
                if (aliveMapping != null && aliveMapping.ModifiedAt < del.DeletedAt)
                {
                    _context.SharedItemErpMappings.Remove(aliveMapping);
                }
            }

            // 6. ItemErpMapping upserts — keyed by LocalItemId (unique), LWW by ModifiedAt.
            foreach (var map in request.ItemErpMappings)
            {
                var existing = await _context.SharedItemErpMappings
                    .FirstOrDefaultAsync(m => m.LocalItemId == map.LocalItemId);
                if (existing == null)
                {
                    _context.SharedItemErpMappings.Add(new SharedItemErpMapping
                    {
                        Id = map.Id,
                        LocalItemId = map.LocalItemId,
                        ErpId = map.ErpId,
                        LastSyncedAt = map.LastSyncedAt,
                        CreatedAt = map.CreatedAt,
                        ModifiedAt = map.ModifiedAt,
                        ModifiedByShopId = request.ShopId
                    });
                    itemErpMappingsUpserted++;
                }
                else if (map.ModifiedAt > existing.ModifiedAt)
                {
                    existing.ErpId = map.ErpId;
                    existing.LastSyncedAt = map.LastSyncedAt;
                    existing.ModifiedAt = map.ModifiedAt;
                    existing.ModifiedByShopId = request.ShopId;
                    itemErpMappingsUpserted++;
                }
            }

            // 6b. Item deletion tombstones (Workstream B, two-phase delete). Each
            // tombstone hard-deletes the SharedItem and cascades to its child rows.
            // Mirrors the ItemCode tombstone handling (section 2b): record idempotently
            // by Id, auto-ack the originating shop, then remove the live registry data.
            foreach (var del in request.ItemDeletions)
            {
                var existingDel = await _context.SharedItemDeletions.FindAsync(del.Id);
                if (existingDel == null)
                {
                    _context.SharedItemDeletions.Add(new SharedItemDeletion
                    {
                        Id = del.Id,
                        DeletedItemId = del.DeletedItemId,
                        DeletedAt = del.DeletedAt,
                        // Server-clock ModifiedAt so the pull-time filter works
                        // regardless of client wall-clock — same as section 2b.
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedByShopId = request.ShopId
                    });
                    itemDeletionsApplied++;

                    // Auto-ack the originating shop — it created and applied this
                    // tombstone locally. Saves pushing a separate ack row.
                    _context.SharedTombstoneAcks.Add(new SharedTombstoneAck
                    {
                        Id = Guid.NewGuid(),
                        TombstoneId = del.Id,
                        TombstoneType = TombstoneTypes.Item,
                        ShopId = request.ShopId,
                        AckedAt = DateTime.UtcNow
                    });
                    tombstoneAcksRecorded++;
                }

                // Cascade-delete every registry row for this item by Id. Registry
                // tables carry no FK constraints (cash-register convention), so order
                // is not strictly required, but children-before-parent reads cleaner.
                var liveCodes = await _context.SharedItemCodes
                    .Where(ic => ic.ItemId == del.DeletedItemId).ToListAsync();
                if (liveCodes.Count > 0) _context.SharedItemCodes.RemoveRange(liveCodes);

                var livePrices = await _context.SharedItemPrices
                    .Where(p => p.ItemId == del.DeletedItemId).ToListAsync();
                if (livePrices.Count > 0) _context.SharedItemPrices.RemoveRange(livePrices);

                var liveMappings = await _context.SharedItemErpMappings
                    .Where(m => m.LocalItemId == del.DeletedItemId).ToListAsync();
                if (liveMappings.Count > 0) _context.SharedItemErpMappings.RemoveRange(liveMappings);

                var liveReadiness = await _context.SharedItemDeleteReadinesses
                    .Where(r => r.ItemId == del.DeletedItemId).ToListAsync();
                if (liveReadiness.Count > 0) _context.SharedItemDeleteReadinesses.RemoveRange(liveReadiness);

                var liveItem = await _context.SharedItems.FindAsync(del.DeletedItemId);
                if (liveItem != null) _context.SharedItems.Remove(liveItem);
            }

            // 7. Tombstone acks from the calling shop. Each entry says "shop X has applied
            // tombstone Y of type T locally." UNIQUE INDEX (TombstoneId, TombstoneType,
            // ShopId) makes re-pushes idempotent. The shop's own acks for tombstones it
            // originated are written above (auto-ack on tombstone insert) — these are for
            // tombstones it received via pull and applied.
            foreach (var ack in request.TombstoneAcks)
            {
                if (string.IsNullOrWhiteSpace(ack.TombstoneType)) continue;
                var alreadyAcked = await _context.SharedTombstoneAcks
                    .AnyAsync(a => a.TombstoneId == ack.TombstoneId
                                && a.TombstoneType == ack.TombstoneType
                                && a.ShopId == request.ShopId);
                if (!alreadyAcked)
                {
                    _context.SharedTombstoneAcks.Add(new SharedTombstoneAck
                    {
                        Id = Guid.NewGuid(),
                        TombstoneId = ack.TombstoneId,
                        TombstoneType = ack.TombstoneType,
                        ShopId = request.ShopId,
                        AckedAt = DateTime.UtcNow
                    });
                    tombstoneAcksRecorded++;
                }
            }

            // 7b. Inter-shop item costs. Per-shop AverageCost for items the calling
            // shop actually purchased. Keyed by (ShopId, ItemId); the ShopId in the
            // row is always the caller's (never trust a foreign ShopId in the payload).
            // LWW by UpdatedAt so a stale re-push can't clobber a fresher value. These
            // rows feed the on-demand GET /itemcost lookup for the OTHER shop only.
            foreach (var cost in request.ItemCosts)
            {
                var existing = await _context.SharedItemCosts
                    .FindAsync(request.ShopId, cost.ItemId);
                if (existing == null)
                {
                    _context.SharedItemCosts.Add(new SharedItemCost
                    {
                        ShopId = request.ShopId,
                        ItemId = cost.ItemId,
                        AverageCost = cost.AverageCost,
                        LastPurchasePrice = cost.LastPurchasePrice,
                        UpdatedAt = cost.UpdatedAt
                    });
                    itemCostsUpserted++;
                }
                else if (cost.UpdatedAt > existing.UpdatedAt)
                {
                    existing.AverageCost = cost.AverageCost;
                    existing.LastPurchasePrice = cost.LastPurchasePrice;
                    existing.UpdatedAt = cost.UpdatedAt;
                    itemCostsUpserted++;
                }
            }

            // 7c. Inter-shop stock snapshots. Per-shop on-hand quantity + AverageCost
            // for items the calling shop has had inventory activity for. Keyed by
            // (ShopId, ItemId); ShopId is always the caller's (never trust a foreign
            // ShopId in the payload). LWW by UpdatedAt so a stale re-push can't clobber
            // a fresher value. These rows feed the on-demand GET /stockacrossshops
            // lookup for the OTHER shop's "Άλλα καταστήματα" panel.
            foreach (var inv in request.Inventories)
            {
                var existing = await _context.SharedInventories
                    .FindAsync(request.ShopId, inv.ItemId);
                if (existing == null)
                {
                    _context.SharedInventories.Add(new SharedInventory
                    {
                        ShopId = request.ShopId,
                        ItemId = inv.ItemId,
                        StockQuantity = inv.StockQuantity,
                        AverageCost = inv.AverageCost,
                        UpdatedAt = inv.UpdatedAt
                    });
                    inventoriesUpserted++;
                }
                else if (inv.UpdatedAt > existing.UpdatedAt)
                {
                    existing.StockQuantity = inv.StockQuantity;
                    existing.AverageCost = inv.AverageCost;
                    existing.UpdatedAt = inv.UpdatedAt;
                    inventoriesUpserted++;
                }
            }

            // 7d. Inter-shop stock transfers (Feature A). The source shop publishes a
            // transfer after its OUT doc commits. Transfers are FINAL — insert once by
            // TransferId, never update (re-push is an idempotent no-op). SourceShopId is
            // forced to the caller's ShopId (never trust a foreign source in the payload).
            foreach (var t in request.StockTransfers)
            {
                if (t.TransferId == Guid.Empty) continue;
                var exists = await _context.SharedStockTransfers.AnyAsync(x => x.TransferId == t.TransferId);
                if (exists) continue;

                _context.SharedStockTransfers.Add(new SharedStockTransfer
                {
                    TransferId = t.TransferId,
                    SourceShopId = request.ShopId,
                    DestShopId = t.DestShopId,
                    TransactionDate = t.TransactionDate,
                    Reference = t.Reference,
                    RequestId = t.RequestId,
                    ClaimId = t.ClaimId,
                    Status = StockTransferStatuses.Pushed,
                    CreatedAt = DateTime.UtcNow,
                    Lines = t.Lines.Select(l => new SharedStockTransferLine
                    {
                        Id = l.Id == Guid.Empty ? Guid.NewGuid() : l.Id,
                        TransferId = t.TransferId,
                        ItemId = l.ItemId,
                        ItemCode = l.ItemCode,
                        Quantity = l.Quantity,
                        CarriedUnitCost = l.CarriedUnitCost,
                        BatchNumber = l.BatchNumber,
                        ExpiryDate = l.ExpiryDate
                    }).ToList()
                });
                transfersUpserted++;

                // A transfer that ships a stock-request claim flips the matching
                // fulfillment Claimed → Shipped (idempotent: only from Claimed; a claim
                // of another shop is never touched).
                if (t.ClaimId is Guid claimId && claimId != Guid.Empty)
                {
                    var fulfillment = await _context.SharedStockRequestFulfillments
                        .FirstOrDefaultAsync(f => f.ClaimId == claimId
                                                  && f.FulfillingShopId == request.ShopId);
                    if (fulfillment != null
                        && fulfillment.Status == StockRequestFulfillmentStatuses.Claimed)
                    {
                        fulfillment.Status = StockRequestFulfillmentStatuses.Shipped;
                        fulfillment.TransferId = t.TransferId;
                        fulfillment.ShippedAt = DateTime.UtcNow;
                    }
                }
            }

            // 7e. Materialization acks from the dest. Flip Status to Materialized once,
            // stamp MaterializedAt. Idempotent — a transfer already Materialized is skipped.
            foreach (var ackId in request.StockTransferAcks)
            {
                var transfer = await _context.SharedStockTransfers.FindAsync(ackId);
                if (transfer != null && transfer.Status != StockTransferStatuses.Materialized)
                {
                    transfer.Status = StockTransferStatuses.Materialized;
                    transfer.MaterializedAt = DateTime.UtcNow;
                    transferAcksApplied++;
                }
            }

            // 7f. Inter-shop stock requests. The requester publishes a request ONCE —
            // insert-immutable by RequestId (re-push is an idempotent no-op), header opens
            // as "Open", RequestingShopId forced to the caller. Progress fields
            // (Fulfilled/CancelledQuantity) are NEVER taken from a push: after this insert
            // the ERP row is the single source of truth for remaining quantity and is only
            // mutated by the atomic claim/release/cancel-remainder endpoints.
            // ONE exception to the no-op: when the requester edited the draft while the
            // original push was in flight, the client keeps it PendingPush and re-sends it
            // next cycle — refresh the lines then, but only while nobody has acted on the
            // request (still Open, zero fulfillment activity).
            foreach (var r in request.StockRequests)
            {
                if (r.RequestId == Guid.Empty) continue;
                var existingRequest = await _context.SharedStockRequests
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.RequestId == r.RequestId);
                if (existingRequest != null)
                {
                    if (existingRequest.RequestingShopId != request.ShopId) continue;
                    if (existingRequest.Status != StockRequestStatuses.Open) continue;
                    var hasFulfillmentActivity =
                        await _context.SharedStockRequestFulfillments
                            .AnyAsync(f => f.RequestId == r.RequestId)
                        || existingRequest.Lines.Any(l =>
                            l.FulfilledQuantity > 0 || l.CancelledQuantity > 0);
                    if (hasFulfillmentActivity)
                    {
                        _logger.LogWarning(
                            "SharedSync Push: stock request {RequestId} re-pushed by shop {ShopId} but already has fulfillment activity; keeping ERP lines.",
                            r.RequestId, request.ShopId);
                        continue;
                    }
                    _context.SharedStockRequestLines.RemoveRange(existingRequest.Lines);
                    // Fresh line Ids on purpose: re-pushed lines may carry the Ids of the
                    // rows being deleted in this same SaveChanges; the client's pull
                    // reconcile falls back to ItemId matching, so new Ids are safe.
                    existingRequest.Lines = r.Lines
                        .Where(l => l.RequestedQuantity > 0)
                        .Select(l => new SharedStockRequestLine
                        {
                            Id = Guid.NewGuid(),
                            RequestId = r.RequestId,
                            ItemId = l.ItemId,
                            ItemCode = l.ItemCode,
                            RequestedQuantity = l.RequestedQuantity,
                            FulfilledQuantity = 0m,
                            CancelledQuantity = 0m
                        }).ToList();
                    existingRequest.Reference = r.Reference;
                    existingRequest.UpdatedAt = DateTime.UtcNow;
                    continue;
                }

                var nowUtc = DateTime.UtcNow;
                _context.SharedStockRequests.Add(new SharedStockRequest
                {
                    RequestId = r.RequestId,
                    RequestingShopId = request.ShopId,
                    TargetShopId = r.TargetShopId,
                    RequestType = string.IsNullOrWhiteSpace(r.RequestType)
                        ? StockRequestTypes.Stock
                        : r.RequestType,
                    RequestDate = r.RequestDate,
                    Reference = r.Reference,
                    Status = StockRequestStatuses.Open,
                    CreatedAt = nowUtc,
                    UpdatedAt = nowUtc,
                    Lines = r.Lines
                        .Where(l => l.RequestedQuantity > 0)
                        .Select(l => new SharedStockRequestLine
                        {
                            Id = l.Id == Guid.Empty ? Guid.NewGuid() : l.Id,
                            RequestId = r.RequestId,
                            ItemId = l.ItemId,
                            ItemCode = l.ItemCode,
                            RequestedQuantity = l.RequestedQuantity,
                            FulfilledQuantity = 0m,
                            CancelledQuantity = 0m
                        }).ToList()
                });
                stockRequestsInserted++;
            }

            await _context.SaveChangesAsync();

            // 8. Purge pass — delete tombstones every active KnownShop has acked.
            // Runs only when we recorded new acks this push (no acks = no purge progress
            // could possibly happen). Separate SaveChanges so a purge failure doesn't
            // roll back the data work above.
            if (tombstoneAcksRecorded > 0)
            {
                var activeShopCount = await _context.KnownShops.CountAsync(s => s.IsActive);
                if (activeShopCount > 0)
                {
                    tombstonesPurged += await PurgeTombstonesByTypeAsync(TombstoneTypes.ItemCode, activeShopCount);
                    tombstonesPurged += await PurgeTombstonesByTypeAsync(TombstoneTypes.ItemPriceLevelMapping, activeShopCount);
                    tombstonesPurged += await PurgeTombstonesByTypeAsync(TombstoneTypes.ItemErpMapping, activeShopCount);
                    tombstonesPurged += await PurgeTombstonesByTypeAsync(TombstoneTypes.Item, activeShopCount);
                    if (tombstonesPurged > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
            }

            _logger.LogInformation(
                "SharedSync Push from shop {ShopId}: {Items} items, {Categories} categories, {VatClasses} vat classes, {MeasureUnits} measure units, {Depts} cashier depts, {ItemCodes} item codes ({IcDel} deletes), {ItemPrices} item prices ({IpDel} deletes), {Mappings} mappings, {MapDeletions} mapping deletions, {ItemDels} item deletions, {Acks} acks, {Purged} purged, {ItemCosts} item costs, {Inventories} inventories, {Transfers} transfers ({TransferAcks} acks), {StockRequests} stock requests",
                request.ShopId, itemsUpserted, categoriesUpserted, vatClassesUpserted, measureUnitsUpserted, cashierDepartmentsUpserted, itemCodesUpserted, itemCodeDeletionsApplied, itemPricesUpserted, itemPriceLevelMappingDeletionsApplied, itemErpMappingsUpserted, itemErpMappingDeletionsApplied, itemDeletionsApplied, tombstoneAcksRecorded, tombstonesPurged, itemCostsUpserted, inventoriesUpserted, transfersUpserted, transferAcksApplied, stockRequestsInserted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SharedSync Push failed for shop {ShopId}", request.ShopId);
            errors.Add(ex.Message);
        }

        return Ok(new SharedSyncPushResponse
        {
            Success = errors.Count == 0,
            ItemsUpserted = itemsUpserted,
            CategoriesUpserted = categoriesUpserted,
            VatClassesUpserted = vatClassesUpserted,
            MeasureUnitsUpserted = measureUnitsUpserted,
            CashierDepartmentsUpserted = cashierDepartmentsUpserted,
            ItemCodesUpserted = itemCodesUpserted,
            ItemPricesUpserted = itemPricesUpserted,
            ItemErpMappingsUpserted = itemErpMappingsUpserted,
            ItemErpMappingDeletionsApplied = itemErpMappingDeletionsApplied,
            ItemCodeDeletionsApplied = itemCodeDeletionsApplied,
            ItemPriceLevelMappingDeletionsApplied = itemPriceLevelMappingDeletionsApplied,
            ItemDeletionsApplied = itemDeletionsApplied,
            TombstoneAcksRecorded = tombstoneAcksRecorded,
            TombstonesPurged = tombstonesPurged,
            ItemCostsUpserted = itemCostsUpserted,
            InventoriesUpserted = inventoriesUpserted,
            TransfersUpserted = transfersUpserted,
            TransferAcksApplied = transferAcksApplied,
            StockRequestsInserted = stockRequestsInserted,
            Errors = errors
        });
    }

    /// <summary>
    /// Synchronous atomic CLAIM against an open stock request — the only way remaining
    /// quantity is ever consumed. The fulfiller calls this BEFORE creating its XFER-OUT
    /// doc (claim-before-ship): a lost race here can never strand shipped goods.
    /// Idempotent by the caller-minted ClaimId — replaying a recorded claim returns the
    /// original success with fresh state. Per line, Quantity must be ≤ remaining unless
    /// ConfirmedOverFulfill is set (operator explicitly confirmed pack/carton rounding);
    /// otherwise 409 with the FRESH request state so the fulfill dialog refreshes in place.
    /// </summary>
    [HttpPost("stockrequests/{requestId}/claim")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> ClaimStockRequest(Guid requestId, [FromBody] StockRequestClaimDto claim)
    {
        if (claim == null || claim.ClaimId == Guid.Empty)
            return BadRequest(new { error = "ClaimId is required" });
        if (string.IsNullOrWhiteSpace(claim.FulfillingShopId))
            return BadRequest(new { error = "FulfillingShopId is required" });
        if (claim.Lines == null || claim.Lines.Count == 0 || claim.Lines.Any(l => l.Quantity <= 0))
            return BadRequest(new { error = "At least one line with a positive quantity is required" });

        await using var tx = await _context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        // Replay of an already-recorded ClaimId → the original outcome (idempotent retry
        // path for the cashier's crash-recovery pass).
        var existing = await _context.SharedStockRequestFulfillments
            .FirstOrDefaultAsync(f => f.ClaimId == claim.ClaimId);
        if (existing != null)
        {
            var replayRequest = await LoadStockRequestAsync(existing.RequestId);
            await tx.CommitAsync();
            return Ok(new StockRequestOperationResultDto
            {
                Success = existing.Status != StockRequestFulfillmentStatuses.Released,
                Error = existing.Status == StockRequestFulfillmentStatuses.Released
                    ? "Claim was already released"
                    : null,
                Request = replayRequest == null ? null : MapStockRequestDto(replayRequest)
            });
        }

        var request = await LoadStockRequestAsync(requestId);
        if (request == null)
            return NotFound(new { error = "Stock request not found" });

        if (request.RequestingShopId == claim.FulfillingShopId)
            return BadRequest(new { error = "A shop cannot fulfill its own request" });
        if (request.TargetShopId != null && request.TargetShopId != claim.FulfillingShopId)
            return BadRequest(new { error = "Request is targeted at another shop" });

        if (request.Status != StockRequestStatuses.Open)
        {
            return Conflict(new StockRequestOperationResultDto
            {
                Success = false,
                Error = "Request is no longer open",
                Request = MapStockRequestDto(request)
            });
        }

        // Aggregate by ItemId (a duplicated item in the body must not double-pass the
        // remaining check), then VALIDATE EVERY line before mutating anything — a
        // Conflict must return the request state exactly as persisted.
        var nowUtc = DateTime.UtcNow;
        var claimLines = claim.Lines
            .GroupBy(l => l.ItemId)
            .Select(g => new { ItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        foreach (var line in claimLines)
        {
            var requestLine = request.Lines.FirstOrDefault(l => l.ItemId == line.ItemId);
            if (requestLine == null)
            {
                return Conflict(new StockRequestOperationResultDto
                {
                    Success = false,
                    Error = $"Item {line.ItemId} is not part of the request",
                    Request = MapStockRequestDto(request)
                });
            }

            var remaining = requestLine.RequestedQuantity
                            - requestLine.FulfilledQuantity
                            - requestLine.CancelledQuantity;
            if (line.Quantity > remaining && !claim.ConfirmedOverFulfill)
            {
                return Conflict(new StockRequestOperationResultDto
                {
                    Success = false,
                    Error = "Requested quantity exceeds the remaining quantity",
                    Request = MapStockRequestDto(request)
                });
            }
        }

        foreach (var line in claimLines)
        {
            var requestLine = request.Lines.First(l => l.ItemId == line.ItemId);
            requestLine.FulfilledQuantity += line.Quantity;
        }

        _context.SharedStockRequestFulfillments.Add(new SharedStockRequestFulfillment
        {
            ClaimId = claim.ClaimId,
            RequestId = request.RequestId,
            FulfillingShopId = claim.FulfillingShopId,
            Status = StockRequestFulfillmentStatuses.Claimed,
            CreatedAt = nowUtc,
            Lines = claimLines.Select(l => new SharedStockRequestFulfillmentLine
            {
                Id = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                ItemId = l.ItemId,
                Quantity = l.Quantity
            }).ToList()
        });

        if (request.Lines.All(l =>
                l.RequestedQuantity - l.FulfilledQuantity - l.CancelledQuantity <= 0))
        {
            request.Status = StockRequestStatuses.Completed;
            request.CompletedAt = nowUtc;
        }
        request.UpdatedAt = nowUtc;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation(
            "StockRequest claim {ClaimId} by shop {ShopId} on request {RequestId}: {Lines} line(s), status now {Status}",
            claim.ClaimId, claim.FulfillingShopId, request.RequestId, claim.Lines.Count, request.Status);

        return Ok(new StockRequestOperationResultDto
        {
            Success = true,
            Request = MapStockRequestDto(request)
        });
    }

    /// <summary>
    /// Compensating RELEASE of a claim whose XFER-OUT doc could not be created (or was
    /// re-driven to release by the cashier's recovery pass). Only a still-Claimed claim can
    /// be released — once the shipping transfer arrived (Shipped) the goods are on their
    /// way and the claim is immutable. Restores the request's remaining quantity and
    /// reopens a request that had auto-completed. Idempotent: releasing a Released claim
    /// returns success.
    /// </summary>
    [HttpPost("stockrequests/claims/{claimId}/release")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> ReleaseStockRequestClaim(Guid claimId)
    {
        await using var tx = await _context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var fulfillment = await _context.SharedStockRequestFulfillments
            .Include(f => f.Lines)
            .FirstOrDefaultAsync(f => f.ClaimId == claimId);
        if (fulfillment == null)
            return NotFound(new { error = "Claim not found" });

        var request = await LoadStockRequestAsync(fulfillment.RequestId);

        if (fulfillment.Status == StockRequestFulfillmentStatuses.Released)
        {
            await tx.CommitAsync();
            return Ok(new StockRequestOperationResultDto
            {
                Success = true,
                Request = request == null ? null : MapStockRequestDto(request)
            });
        }

        if (fulfillment.Status == StockRequestFulfillmentStatuses.Shipped)
        {
            return Conflict(new StockRequestOperationResultDto
            {
                Success = false,
                Error = "Claim has already been shipped",
                Request = request == null ? null : MapStockRequestDto(request)
            });
        }

        var nowUtc = DateTime.UtcNow;
        if (request != null)
        {
            foreach (var line in fulfillment.Lines)
            {
                var requestLine = request.Lines.FirstOrDefault(l => l.ItemId == line.ItemId);
                if (requestLine != null)
                {
                    requestLine.FulfilledQuantity =
                        Math.Max(0m, requestLine.FulfilledQuantity - line.Quantity);
                }
            }

            if (request.Status == StockRequestStatuses.Completed
                && request.Lines.Any(l =>
                    l.RequestedQuantity - l.FulfilledQuantity - l.CancelledQuantity > 0))
            {
                request.Status = StockRequestStatuses.Open;
                request.CompletedAt = null;
            }
            request.UpdatedAt = nowUtc;
        }

        fulfillment.Status = StockRequestFulfillmentStatuses.Released;
        fulfillment.ReleasedAt = nowUtc;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation(
            "StockRequest claim {ClaimId} released (request {RequestId})",
            claimId, fulfillment.RequestId);

        return Ok(new StockRequestOperationResultDto
        {
            Success = true,
            Request = request == null ? null : MapStockRequestDto(request)
        });
    }

    /// <summary>
    /// Requester-side CANCEL of a request's unfulfilled remainder. Serialized against
    /// claims (a claim granted before this call keeps its goods; one arriving after loses
    /// the race and gets 409). Per line CancelledQuantity absorbs max(0, remaining); the
    /// header closes as Completed when anything was ever fulfilled, Cancelled otherwise.
    /// Idempotent: cancelling an already-closed request returns success with fresh state.
    /// </summary>
    [HttpPost("stockrequests/{requestId}/cancelremainder")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> CancelStockRequestRemainder(Guid requestId, [FromQuery] string shopId)
    {
        if (string.IsNullOrWhiteSpace(shopId))
            return BadRequest(new { error = "shopId is required" });

        await using var tx = await _context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var request = await LoadStockRequestAsync(requestId);
        if (request == null)
            return NotFound(new { error = "Stock request not found" });

        if (request.RequestingShopId != shopId)
            return BadRequest(new { error = "Only the requesting shop can cancel its request" });

        if (request.Status != StockRequestStatuses.Open)
        {
            await tx.CommitAsync();
            return Ok(new StockRequestOperationResultDto
            {
                Success = true,
                Request = MapStockRequestDto(request)
            });
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var line in request.Lines)
        {
            var remaining = line.RequestedQuantity - line.FulfilledQuantity - line.CancelledQuantity;
            if (remaining > 0)
                line.CancelledQuantity += remaining;
        }

        request.Status = request.Lines.Any(l => l.FulfilledQuantity > 0)
            ? StockRequestStatuses.Completed
            : StockRequestStatuses.Cancelled;
        request.CompletedAt = nowUtc;
        request.UpdatedAt = nowUtc;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation(
            "StockRequest {RequestId} remainder cancelled by shop {ShopId}, status now {Status}",
            requestId, shopId, request.Status);

        return Ok(new StockRequestOperationResultDto
        {
            Success = true,
            Request = MapStockRequestDto(request)
        });
    }

    private Task<SharedStockRequest?> LoadStockRequestAsync(Guid requestId) =>
        _context.SharedStockRequests
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);

    private static SharedStockRequestDto MapStockRequestDto(SharedStockRequest r) => new()
    {
        RequestId = r.RequestId,
        RequestingShopId = r.RequestingShopId,
        TargetShopId = r.TargetShopId,
        RequestType = r.RequestType,
        RequestDate = r.RequestDate,
        Reference = r.Reference,
        Status = r.Status,
        UpdatedAt = r.UpdatedAt,
        CompletedAt = r.CompletedAt,
        Lines = r.Lines.Select(l => new SharedStockRequestLineDto
        {
            Id = l.Id,
            ItemId = l.ItemId,
            ItemCode = l.ItemCode,
            RequestedQuantity = l.RequestedQuantity,
            FulfilledQuantity = l.FulfilledQuantity,
            CancelledQuantity = l.CancelledQuantity
        }).ToList()
    };

    /// <summary>
    /// On-demand cross-shop cost lookup. Returns the most recently-updated cost
    /// published by a shop OTHER than the requester for the given item, or 404 when
    /// none exists. Used by the sale summary diary "borrow estimated cost" action for
    /// items a shop sells but never purchased. requestingShopId is excluded so a shop
    /// never borrows its own (absent) cost back.
    /// </summary>
    [HttpGet("itemcost/{itemId}")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> GetItemCost(Guid itemId, [FromQuery] string requestingShopId)
    {
        if (string.IsNullOrWhiteSpace(requestingShopId))
            return BadRequest(new { error = "requestingShopId is required" });

        var cost = await _context.SharedItemCosts
            .Where(c => c.ItemId == itemId && c.ShopId != requestingShopId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new SharedItemCostDto
            {
                ItemId = c.ItemId,
                AverageCost = c.AverageCost,
                LastPurchasePrice = c.LastPurchasePrice,
                UpdatedAt = c.UpdatedAt,
                ShopId = c.ShopId
            })
            .FirstOrDefaultAsync();

        if (cost == null)
            return NotFound();

        return Ok(cost);
    }

    /// <summary>
    /// On-demand cross-shop stock query. Returns the on-hand snapshot every shop
    /// OTHER than the requester has published for the given item (most-recent row
    /// per shop), so the "Άλλα καταστήματα" panel can show how much stock peers
    /// hold. requestingShopId is excluded so a shop never sees its own (locally
    /// authoritative) stock echoed back. Returns an empty list when no peer has
    /// published — never 404, since "no other shop has stock" is a valid answer.
    /// </summary>
    [HttpGet("stockacrossshops/{itemId}")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> GetStockAcrossShops(Guid itemId, [FromQuery] string requestingShopId)
    {
        if (string.IsNullOrWhiteSpace(requestingShopId))
            return BadRequest(new { error = "requestingShopId is required" });

        // Fetch the raw inventory rows first — a plain, always-translatable query.
        // (Do NOT correlate a KnownShops subquery inside this projection: that form
        // throws at runtime on some EF Core versions and hard-fails the whole request
        // with a 500 if the KnownShops table is absent/incompatible on this DB.)
        var rows = await _context.SharedInventories
            .Where(i => i.ItemId == itemId && i.ShopId != requestingShopId)
            .OrderBy(i => i.ShopId)
            .Select(i => new SharedInventoryDto
            {
                ItemId = i.ItemId,
                StockQuantity = i.StockQuantity,
                AverageCost = i.AverageCost,
                UpdatedAt = i.UpdatedAt,
                ShopId = i.ShopId,
                ShopName = i.ShopId   // default label; upgraded below when available
            })
            .ToListAsync();

        // Resolve human-readable shop names in a separate, failure-isolated pass so a
        // missing/empty KnownShops registry degrades to raw ShopIds instead of a 500.
        try
        {
            var shopIds = rows.Select(r => r.ShopId).Distinct().ToList();
            var names = await _context.KnownShops
                .Where(s => shopIds.Contains(s.ShopId) && s.DisplayName != null)
                .Select(s => new { s.ShopId, s.DisplayName })
                .ToListAsync();
            var nameMap = names.ToDictionary(n => n.ShopId, n => n.DisplayName!);
            foreach (var r in rows)
                if (nameMap.TryGetValue(r.ShopId, out var dn) && !string.IsNullOrWhiteSpace(dn))
                    r.ShopName = dn;
        }
        catch (Exception ex)
        {
            // Names are cosmetic; never let the registry lookup break the stock query.
            _logger.LogWarning(ex, "GetStockAcrossShops: KnownShops name resolution failed; returning raw ShopIds.");
        }

        return Ok(rows);
    }

    /// <summary>
    /// Lists the shops known to the registry (KnownShops), excluding the requester,
    /// so a shop can enumerate its peers and seed one inter-shop transactor per peer.
    /// DisplayName carries the peer's CompanyCode (populated from the push heartbeat).
    /// Only active shops are returned. requestingShopId is required and excluded.
    /// </summary>
    [HttpGet("knownshops")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> GetKnownShops([FromQuery] string requestingShopId)
    {
        if (string.IsNullOrWhiteSpace(requestingShopId))
            return BadRequest(new { error = "requestingShopId is required" });

        var shops = await _context.KnownShops
            .Where(s => s.IsActive && s.ShopId != requestingShopId)
            .OrderBy(s => s.ShopId)
            .Select(s => new KnownShopDto
            {
                ShopId = s.ShopId,
                DisplayName = s.DisplayName,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(shops);
    }

    /// <summary>
    /// Hard-deletes tombstones of a given type that every active KnownShop has acked.
    /// Also removes the matching SharedTombstoneAck rows. Returns the count of
    /// tombstones (not acks) deleted. Caller is responsible for SaveChangesAsync.
    /// </summary>
    private async Task<int> PurgeTombstonesByTypeAsync(string tombstoneType, int activeShopCount)
    {
        // Group acks by TombstoneId, count distinct acks from currently-active shops.
        // A tombstone is purgeable when that count >= activeShopCount.
        var purgeableIds = await _context.SharedTombstoneAcks
            .Where(a => a.TombstoneType == tombstoneType
                     && _context.KnownShops.Any(s => s.ShopId == a.ShopId && s.IsActive))
            .GroupBy(a => a.TombstoneId)
            .Where(g => g.Count() >= activeShopCount)
            .Select(g => g.Key)
            .ToListAsync();

        if (purgeableIds.Count == 0) return 0;

        // Remove the matching acks (including any from inactive shops, since the tombstone
        // is gone anyway). Use IN-set delete for batch efficiency.
        var acksToRemove = await _context.SharedTombstoneAcks
            .Where(a => a.TombstoneType == tombstoneType && purgeableIds.Contains(a.TombstoneId))
            .ToListAsync();
        _context.SharedTombstoneAcks.RemoveRange(acksToRemove);

        // Delete from the actual tombstone table for this type.
        if (tombstoneType == TombstoneTypes.ItemCode)
        {
            var tombs = await _context.SharedItemCodeDeletions
                .Where(d => purgeableIds.Contains(d.Id))
                .ToListAsync();
            _context.SharedItemCodeDeletions.RemoveRange(tombs);
            return tombs.Count;
        }
        if (tombstoneType == TombstoneTypes.ItemPriceLevelMapping)
        {
            var tombs = await _context.SharedItemPriceLevelMappingDeletions
                .Where(d => purgeableIds.Contains(d.Id))
                .ToListAsync();
            _context.SharedItemPriceLevelMappingDeletions.RemoveRange(tombs);
            return tombs.Count;
        }
        if (tombstoneType == TombstoneTypes.ItemErpMapping)
        {
            var tombs = await _context.SharedItemErpMappingDeletions
                .Where(d => purgeableIds.Contains(d.Id))
                .ToListAsync();
            _context.SharedItemErpMappingDeletions.RemoveRange(tombs);
            return tombs.Count;
        }
        if (tombstoneType == TombstoneTypes.Item)
        {
            var tombs = await _context.SharedItemDeletions
                .Where(d => purgeableIds.Contains(d.Id))
                .ToListAsync();
            _context.SharedItemDeletions.RemoveRange(tombs);
            return tombs.Count;
        }
        return 0;
    }

    /// <summary>
    /// Full dump of all shared items and reference data.
    /// Used for initial setup or recovery.
    /// </summary>
    [HttpGet("pull-all")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> PullAll()
    {
        var serverTimestamp = DateTime.UtcNow;

        var items = await _context.SharedItems
            .Select(i => new SharedItemDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                Active = i.Active,
                ItemCategoryId = i.ItemCategoryId,
                VatClassId = i.VatClassId,
                MainUnitId = i.MainUnitId,
                CashierDepartmentId = i.CashierDepartmentId,
                ItemNature = i.ItemNature,
                ItemType = i.ItemType,
                ManufacturerCode = i.ManufacturerCode,
                UpcCode = i.UpcCode,
                EanCode = i.EanCode,
                UseBatchTracking = i.UseBatchTracking,
                DepositItemId = i.DepositItemId,
                IsDepositItem = i.IsDepositItem,
                DeleteRequested = i.DeleteRequested,
                DeleteRequestedByShopId = i.DeleteRequestedByShopId,
                DeleteRequestedAt = i.DeleteRequestedAt,
                ModifiedAt = i.ModifiedAt,
                ModifiedByShopId = i.ModifiedByShopId,
                Version = i.Version
            })
            .ToListAsync();

        var categories = await _context.SharedItemCategories
            .Select(c => new SharedItemCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ModifiedAt = c.ModifiedAt
            })
            .ToListAsync();

        var vatClasses = await _context.SharedVatClasses
            .Select(v => new SharedVatClassDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Rate = v.Rate,
                ModifiedAt = v.ModifiedAt
            })
            .ToListAsync();

        var measureUnits = await _context.SharedMeasureUnits
            .Select(m => new SharedMeasureUnitDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                ModifiedAt = m.ModifiedAt
            })
            .ToListAsync();

        var cashierDepartments = await _context.SharedCashierDepartments
            .Select(d => new SharedCashierDepartmentDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                VatClassId = d.VatClassId,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemCodes = await _context.SharedItemCodes
            .Select(ic => new SharedItemCodeDto
            {
                Id = ic.Id,
                ItemId = ic.ItemId,
                CodeType = ic.CodeType,
                Code = ic.Code,
                MeasureUnitId = ic.MeasureUnitId,
                Quantity = ic.Quantity,
                CreatedAt = ic.CreatedAt,
                LastUsedAt = ic.LastUsedAt
            })
            .ToListAsync();

        var itemPrices = await _context.SharedItemPrices
            .Select(p => new SharedItemPriceDto
            {
                Id = p.Id,
                ItemId = p.ItemId,
                PriceLevelId = p.PriceLevelId,
                NetPrice = p.NetPrice,
                BrutPrice = p.BrutPrice,
                Markup = p.Markup,
                IsOverridden = p.IsOverridden,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                IsActive = p.IsActive,
                ModifiedAt = p.ModifiedAt,
                ModifiedByShopId = p.ModifiedByShopId
            })
            .ToListAsync();

        var itemErpMappings = await _context.SharedItemErpMappings
            .Select(m => new SharedItemErpMappingDto
            {
                Id = m.Id,
                LocalItemId = m.LocalItemId,
                ErpId = m.ErpId,
                LastSyncedAt = m.LastSyncedAt,
                CreatedAt = m.CreatedAt,
                ModifiedAt = m.ModifiedAt,
                ModifiedByShopId = m.ModifiedByShopId
            })
            .ToListAsync();

        var itemErpMappingDeletions = await _context.SharedItemErpMappingDeletions
            .Select(d => new SharedItemErpMappingDeletionDto
            {
                Id = d.Id,
                LocalItemId = d.LocalItemId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemCodeDeletions = await _context.SharedItemCodeDeletions
            .Select(d => new SharedItemCodeDeletionDto
            {
                Id = d.Id,
                DeletedItemCodeId = d.DeletedItemCodeId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemPriceLevelMappingDeletions = await _context.SharedItemPriceLevelMappingDeletions
            .Select(d => new SharedItemPriceLevelMappingDeletionDto
            {
                Id = d.Id,
                DeletedMappingId = d.DeletedMappingId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        var itemDeletions = await _context.SharedItemDeletions
            .Select(d => new SharedItemDeletionDto
            {
                Id = d.Id,
                DeletedItemId = d.DeletedItemId,
                DeletedAt = d.DeletedAt,
                ModifiedAt = d.ModifiedAt,
                ModifiedByShopId = d.ModifiedByShopId
            })
            .ToListAsync();

        return Ok(new SharedSyncPullResponse
        {
            Items = items,
            Categories = categories,
            VatClasses = vatClasses,
            MeasureUnits = measureUnits,
            CashierDepartments = cashierDepartments,
            ItemCodes = itemCodes,
            ItemPrices = itemPrices,
            ItemErpMappings = itemErpMappings,
            ItemErpMappingDeletions = itemErpMappingDeletions,
            ItemCodeDeletions = itemCodeDeletions,
            ItemPriceLevelMappingDeletions = itemPriceLevelMappingDeletions,
            ItemDeletions = itemDeletions,
            ServerTimestamp = serverTimestamp
        });
    }
}
