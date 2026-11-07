using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Sync;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Controllers;

/// <summary>
/// Supplier codes shared between the shops. Separate endpoints, called after the shop's catalog sync,
/// so a problem here can never fail or slow down the sync of items and prices. Older cash registers
/// never call them.
/// </summary>
[Route("api/sharedsync/suppliercodes")]
[ApiController]
public class SharedSupplierCodesController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<SharedSupplierCodesController> _logger;

    public SharedSupplierCodesController(ApiDbContext context, ILogger<SharedSupplierCodesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Codes accepted since the given server time, without the requesting shop's own. With no time, every
    /// code including its own: a first run, or a shop rebuilding its state after a restore.
    /// </summary>
    [HttpGet("pull")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult<SupplierCodePullResponseDto>> Pull(
        [FromQuery] DateTime? since, [FromQuery] string shopId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(shopId))
            return BadRequest(new { error = "shopId is required" });

        // Before the read, so a code accepted during it comes again rather than never.
        var serverTimestamp = DateTime.UtcNow;

        var query = _context.SharedSupplierItemCodes.AsNoTracking();
        if (since is { } from)
            query = query.Where(c => c.ReceivedAt > from && c.ModifiedByShopId != shopId);

        var rows = await query.OrderBy(c => c.ReceivedAt).ThenBy(c => c.Id).ToListAsync(ct);
        return new SupplierCodePullResponseDto
        {
            Codes = rows.Select(SupplierCodeHubRules.ToDto).ToList(),
            ServerTimestamp = serverTimestamp
        };
    }

    /// <summary>
    /// Stores what the hub can accept and answers for every code, at the same position. A refused code
    /// changes nothing and comes back with the hub's mapping. On a write clash nothing is stored and the
    /// answer is 409: the shop still has the codes as unsent and tries again on its next sync.
    /// </summary>
    [HttpPost("push")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult<SupplierCodePushResponseDto>> Push(
        [FromBody] SupplierCodePushRequestDto request, CancellationToken ct = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ShopId))
            return BadRequest(new { error = "shopId is required" });

        var now = DateTime.UtcNow;
        var vats = request.Codes
            .Where(SupplierCodeHubRules.IsValid)
            .Select(c => c.SupplierVat)
            .Distinct()
            .ToList();
        var known = (await _context.SharedSupplierItemCodes
                .Where(c => vats.Contains(c.SupplierVat))
                .ToListAsync(ct))
            .GroupBy(c => KeyOf(c))
            .ToDictionary(g => g.Key, g => g.First());

        var results = new List<SupplierCodePushResultDto>(request.Codes.Count);
        foreach (var code in request.Codes)
        {
            if (!SupplierCodeHubRules.IsValid(code))
            {
                results.Add(new SupplierCodePushResultDto { Invalid = true });
                continue;
            }

            var key = KeyOf(code);
            known.TryGetValue(key, out var row);
            var outcome = SupplierCodeHubRules.Apply(row, code, request.ShopId, out var applied);

            if (outcome == SupplierCodePushOutcome.Inserted)
            {
                _context.SharedSupplierItemCodes.Add(applied);
                known[key] = applied;
            }
            if (outcome is SupplierCodePushOutcome.Inserted or SupplierCodePushOutcome.Updated)
                applied.ReceivedAt = now;

            results.Add(outcome == SupplierCodePushOutcome.Refused
                ? new SupplierCodePushResultDto { Version = applied.Version, Hub = SupplierCodeHubRules.ToDto(applied) }
                : new SupplierCodePushResultDto { Accepted = true, Version = applied.Version });
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // The other shop pushed the same key at the same moment.
            _logger.LogWarning(ex, "Supplier codes push from {ShopId} clashed; the shop retries on its next sync.", request.ShopId);
            return StatusCode(StatusCodes.Status409Conflict, new { error = "Supplier codes changed during the push." });
        }

        return new SupplierCodePushResponseDto { Results = results, ServerTimestamp = now };
    }

    private static (string, int, string) KeyOf(SharedSupplierItemCode c) =>
        (c.SupplierVat, c.MatchKind, SupplierCodeHubRules.LookupKey(c.MatchKey));

    private static (string, int, string) KeyOf(SupplierCodePushDto c) =>
        (c.SupplierVat, c.MatchKind, SupplierCodeHubRules.LookupKey(c.MatchKey));
}
