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
                ItemNature = i.ItemNature,
                ItemType = i.ItemType,
                ManufacturerCode = i.ManufacturerCode,
                UpcCode = i.UpcCode,
                EanCode = i.EanCode,
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

        _logger.LogInformation(
            "SharedSync Pull for shop {ShopId} since {Since}: {Items} items, {Categories} categories, {VatClasses} vat classes, {MeasureUnits} measure units, {ItemCodes} item codes, {ItemPrices} item prices, {Mappings} mappings, {MapDeletions} mapping deletions",
            shopId, since, items.Count, categories.Count, vatClasses.Count, measureUnits.Count, itemCodes.Count, itemPrices.Count, itemErpMappings.Count, itemErpMappingDeletions.Count);

        return Ok(new SharedSyncPullResponse
        {
            Items = items,
            Categories = categories,
            VatClasses = vatClasses,
            MeasureUnits = measureUnits,
            ItemCodes = itemCodes,
            ItemPrices = itemPrices,
            ItemErpMappings = itemErpMappings,
            ItemErpMappingDeletions = itemErpMappingDeletions,
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
        int itemsUpserted = 0;
        int itemCodesUpserted = 0;
        int itemPricesUpserted = 0;
        int itemErpMappingsUpserted = 0;
        int itemErpMappingDeletionsApplied = 0;

        try
        {
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
                        ItemNature = item.ItemNature,
                        ItemType = item.ItemType,
                        ManufacturerCode = item.ManufacturerCode,
                        UpcCode = item.UpcCode,
                        EanCode = item.EanCode,
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
                    existing.ItemNature = item.ItemNature;
                    existing.ItemType = item.ItemType;
                    existing.ManufacturerCode = item.ManufacturerCode;
                    existing.UpcCode = item.UpcCode;
                    existing.EanCode = item.EanCode;
                    existing.ModifiedAt = item.ModifiedAt;
                    existing.ModifiedByShopId = request.ShopId;
                    existing.Version++;
                    itemsUpserted++;
                }
            }

            // 3. Item codes — replace all codes for pushed items
            var pushedItemIds = request.Items.Select(i => i.Id).ToHashSet();
            if (pushedItemIds.Count > 0 && request.ItemCodes.Count > 0)
            {
                var existingCodes = await _context.SharedItemCodes
                    .Where(ic => pushedItemIds.Contains(ic.ItemId))
                    .ToListAsync();
                _context.SharedItemCodes.RemoveRange(existingCodes);

                foreach (var ic in request.ItemCodes)
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
                    itemCodesUpserted++;
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
                        ModifiedAt = del.ModifiedAt,
                        ModifiedByShopId = request.ShopId
                    });
                    itemErpMappingDeletionsApplied++;
                }

                // Remove the alive mapping for this LocalItemId if its ModifiedAt is
                // older than the deletion — otherwise another shop has already
                // re-mapped past this tombstone and we leave it alone.
                var aliveMapping = await _context.SharedItemErpMappings
                    .FirstOrDefaultAsync(m => m.LocalItemId == del.LocalItemId);
                if (aliveMapping != null && aliveMapping.ModifiedAt < del.ModifiedAt)
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

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SharedSync Push from shop {ShopId}: {Items} items, {Categories} categories, {VatClasses} vat classes, {MeasureUnits} measure units, {ItemCodes} item codes, {ItemPrices} item prices, {Mappings} mappings, {MapDeletions} mapping deletions",
                request.ShopId, itemsUpserted, categoriesUpserted, vatClassesUpserted, measureUnitsUpserted, itemCodesUpserted, itemPricesUpserted, itemErpMappingsUpserted, itemErpMappingDeletionsApplied);
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
            ItemCodesUpserted = itemCodesUpserted,
            ItemPricesUpserted = itemPricesUpserted,
            ItemErpMappingsUpserted = itemErpMappingsUpserted,
            ItemErpMappingDeletionsApplied = itemErpMappingDeletionsApplied,
            Errors = errors
        });
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
                ItemNature = i.ItemNature,
                ItemType = i.ItemType,
                ManufacturerCode = i.ManufacturerCode,
                UpcCode = i.UpcCode,
                EanCode = i.EanCode,
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

        return Ok(new SharedSyncPullResponse
        {
            Items = items,
            Categories = categories,
            VatClasses = vatClasses,
            MeasureUnits = measureUnits,
            ItemCodes = itemCodes,
            ItemPrices = itemPrices,
            ItemErpMappings = itemErpMappings,
            ItemErpMappingDeletions = itemErpMappingDeletions,
            ServerTimestamp = serverTimestamp
        });
    }
}
