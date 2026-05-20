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
/// Bidirectional notification channel for the two-phase item delete (Workstream B/C).
/// Shops poll this on a timer, independent of the full SharedItemSync: the request
/// carries the shop's readiness rows for its DeleteRequested items, the response
/// carries the cross-shop pending-delete summary. The call also bumps
/// KnownShops.LastSeenAt, doubling as a liveness heartbeat.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(ApiDbContext context, ILogger<NotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("poll")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> Poll([FromBody] NotificationPollRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ShopId))
            return BadRequest(new { error = "ShopId is required" });

        var now = DateTime.UtcNow;

        // 1. Register / heartbeat the polling shop. Mirrors SharedSyncController's
        //    auto-register; the poll is a liveness signal even with zero readiness rows.
        var knownShop = await _context.KnownShops.FindAsync(request.ShopId);
        if (knownShop == null)
        {
            _context.KnownShops.Add(new KnownShop
            {
                ShopId = request.ShopId,
                FirstSeenAt = now,
                LastSeenAt = now,
                IsActive = true
            });
        }
        else
        {
            knownShop.LastSeenAt = now;
        }

        // 2. Upsert this shop's readiness rows (incoming-wins on ReportedAt). A shop
        //    is trusted only for rows tagged with its own ShopId.
        foreach (var r in request.Readiness)
        {
            if (!string.Equals(r.ShopId, request.ShopId, StringComparison.Ordinal))
                continue;
            var existing = await _context.SharedItemDeleteReadinesses
                .FindAsync(r.ItemId, request.ShopId);
            if (existing == null)
            {
                _context.SharedItemDeleteReadinesses.Add(new SharedItemDeleteReadiness
                {
                    ItemId = r.ItemId,
                    ShopId = request.ShopId,
                    LocalFkCount = r.LocalFkCount,
                    ReportedAt = r.ReportedAt
                });
            }
            else if (r.ReportedAt >= existing.ReportedAt)
            {
                existing.LocalFkCount = r.LocalFkCount;
                existing.ReportedAt = r.ReportedAt;
            }
        }

        await _context.SaveChangesAsync();

        // 3. Build the cross-shop pending-delete summary.
        var pendingItems = await _context.SharedItems
            .Where(i => i.DeleteRequested)
            .Select(i => new
            {
                i.Id, i.Code, i.Name, i.DeleteRequestedByShopId, i.DeleteRequestedAt
            })
            .ToListAsync();

        var pendingDeletes = new List<PendingDeleteItemDto>();
        if (pendingItems.Count > 0)
        {
            var activeShopIds = await _context.KnownShops
                .Where(s => s.IsActive)
                .Select(s => s.ShopId)
                .ToListAsync();

            var pendingIds = pendingItems.Select(p => p.Id).ToList();
            var readinessRows = await _context.SharedItemDeleteReadinesses
                .Where(r => pendingIds.Contains(r.ItemId))
                .ToListAsync();

            foreach (var p in pendingItems)
            {
                var rows = readinessRows.Where(r => r.ItemId == p.Id).ToList();
                var thisShopRow = rows.FirstOrDefault(r => r.ShopId == request.ShopId);
                // Ready only when every active shop has a zero-count readiness row.
                var allReady = activeShopIds.Count > 0 && activeShopIds.All(sid =>
                    rows.Any(r => r.ShopId == sid && r.LocalFkCount == 0));
                pendingDeletes.Add(new PendingDeleteItemDto
                {
                    ItemId = p.Id,
                    ItemCode = p.Code,
                    ItemName = p.Name,
                    RequestedByShopId = p.DeleteRequestedByShopId ?? string.Empty,
                    RequestedAt = p.DeleteRequestedAt,
                    ThisShopReady = thisShopRow != null && thisShopRow.LocalFkCount == 0,
                    AllShopsReady = allReady,
                    Readiness = rows.Select(r => new ItemDeleteReadinessDto
                    {
                        ItemId = r.ItemId,
                        ShopId = r.ShopId,
                        LocalFkCount = r.LocalFkCount,
                        ReportedAt = r.ReportedAt
                    }).ToList()
                });
            }
        }

        _logger.LogInformation(
            "Notifications poll from shop {ShopId}: {Readiness} readiness rows in, {Pending} pending deletes out",
            request.ShopId, request.Readiness.Count, pendingDeletes.Count);

        return Ok(new NotificationSummaryDto
        {
            AsOf = now,
            PendingDeleteCount = pendingDeletes.Count,
            PendingDeletes = pendingDeletes
        });
    }
}
