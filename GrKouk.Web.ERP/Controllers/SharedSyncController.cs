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
        var includedItemIds = items.Select(i => i.Id).ToHashSet();
        var referencedItemIds = itemErpMappings.Select(m => m.LocalItemId)
            .Concat(itemPrices.Select(p => p.ItemId))
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

        // For item codes, return codes belonging to items in the (now FK-complete) batch
        var allItemIds = items.Select(i => i.Id).ToHashSet();
        var itemCodes = await _context.SharedItemCodes
            .Where(ic => allItemIds.Contains(ic.ItemId))
            .Select(ic => new SharedItemCodeDto
            {
                Id = ic.Id,
                ItemId = ic.ItemId,
                CodeType = ic.CodeType,
                Code = ic.Code,
                MeasureUnitId = ic.MeasureUnitId,
                Quantity = ic.Quantity
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
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
            else
            {
                knownShop.LastSeenAt = DateTime.UtcNow;
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
                        Quantity = ic.Quantity
                    });
                }
                else
                {
                    existing.ItemId = ic.ItemId;
                    existing.CodeType = ic.CodeType;
                    existing.Code = ic.Code;
                    existing.MeasureUnitId = ic.MeasureUnitId;
                    existing.Quantity = ic.Quantity;
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
                "SharedSync Push from shop {ShopId}: {Items} items, {Categories} categories, {VatClasses} vat classes, {MeasureUnits} measure units, {Depts} cashier depts, {ItemCodes} item codes ({IcDel} deletes), {ItemPrices} item prices ({IpDel} deletes), {Mappings} mappings, {MapDeletions} mapping deletions, {ItemDels} item deletions, {Acks} acks, {Purged} purged",
                request.ShopId, itemsUpserted, categoriesUpserted, vatClassesUpserted, measureUnitsUpserted, cashierDepartmentsUpserted, itemCodesUpserted, itemCodeDeletionsApplied, itemPricesUpserted, itemPriceLevelMappingDeletionsApplied, itemErpMappingsUpserted, itemErpMappingDeletionsApplied, itemDeletionsApplied, tombstoneAcksRecorded, tombstonesPurged);
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
            Errors = errors
        });
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
                Quantity = ic.Quantity
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
