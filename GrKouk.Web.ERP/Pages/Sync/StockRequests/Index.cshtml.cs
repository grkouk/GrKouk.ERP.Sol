using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Pages.Sync.StockRequests
{
    /// <summary>
    /// Admin view of the inter-shop stock requests held by the registry: per-request
    /// progress, its fulfillments (who ships what, linked transfer), stuck-claim
    /// highlighting, and the housekeeping actions the shops cannot do themselves —
    /// clear closed requests, admin cancel-remainder, void a stuck claim.
    /// Mutations mirror the SharedSyncController endpoints' semantics exactly
    /// (Serializable transactions — they race with live shop claims).
    /// </summary>
    [Authorize(Roles = "Admin,Operator")]
    public class IndexModel : PageModel
    {
        /// <summary>A fulfillment still Claimed after this many days is flagged as stuck
        /// (transfer never arrived — the shop's recovery failed). Same threshold family as
        /// the cashier-side transfer watchdog.</summary>
        public const int StuckClaimThresholdDays = 3;

        private readonly ApiDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApiDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── Display rows ───────────────────────────────────────────────

        public class FulfillmentRow
        {
            public Guid ClaimId { get; set; }
            public string ShopName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal TotalQuantity { get; set; }
            public Guid? TransferId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ShippedAt { get; set; }
            public bool IsStuck { get; set; }
        }

        public class LineRow
        {
            public string? ItemCode { get; set; }
            public decimal Requested { get; set; }
            public decimal Fulfilled { get; set; }
            public decimal Cancelled { get; set; }
            public decimal Remaining { get; set; }
        }

        public class RequestRow
        {
            public Guid RequestId { get; set; }
            public string ShopName { get; set; } = string.Empty;
            public DateTime RequestDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? Reference { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public List<LineRow> Lines { get; set; } = new();
            public List<FulfillmentRow> Fulfillments { get; set; } = new();
            public bool HasStuckClaim { get; set; }
            public bool IsOpen => Status == StockRequestStatuses.Open;
        }

        public List<RequestRow> Requests { get; set; } = new();
        public int ClosedCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string? ShopFilter { get; set; }

        public List<(string ShopId, string Name)> ShopOptions { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        // ─── Load ───────────────────────────────────────────────────────

        public async Task OnGetAsync()
        {
            var query = _context.SharedStockRequests
                .AsNoTracking()
                .Include(r => r.Lines)
                .Include(r => r.Fulfillments)
                .ThenInclude(f => f.Lines)
                .AsQueryable();

            if (StatusFilter is StockRequestStatuses.Open
                or StockRequestStatuses.Completed
                or StockRequestStatuses.Cancelled)
            {
                query = query.Where(r => r.Status == StatusFilter);
            }
            if (!string.IsNullOrWhiteSpace(ShopFilter))
            {
                query = query.Where(r => r.RequestingShopId == ShopFilter);
            }

            var rows = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            ClosedCount = await _context.SharedStockRequests
                .CountAsync(r => r.Status != StockRequestStatuses.Open);

            // Shop display names — failure-isolated two-pass lookup (raw ids if the
            // KnownShops read fails), same hardening as GetStockAcrossShops.
            var shopIds = rows.Select(r => r.RequestingShopId)
                .Concat(rows.SelectMany(r => r.Fulfillments.Select(f => f.FulfillingShopId)))
                .Distinct()
                .ToList();
            var nameById = new Dictionary<string, string>();
            try
            {
                nameById = await _context.KnownShops
                    .AsNoTracking()
                    .Where(s => shopIds.Contains(s.ShopId) && s.DisplayName != null)
                    .ToDictionaryAsync(s => s.ShopId, s => s.DisplayName!);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "KnownShops name lookup failed; showing raw shop ids.");
            }
            string NameFor(string shopId) =>
                nameById.TryGetValue(shopId, out var n) ? n : shopId;

            var stuckCutoff = DateTime.UtcNow.AddDays(-StuckClaimThresholdDays);
            Requests = rows.Select(r => new RequestRow
            {
                RequestId = r.RequestId,
                ShopName = NameFor(r.RequestingShopId),
                RequestDate = r.RequestDate,
                Status = r.Status,
                Reference = r.Reference,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt,
                Lines = r.Lines.Select(l => new LineRow
                {
                    ItemCode = l.ItemCode,
                    Requested = l.RequestedQuantity,
                    Fulfilled = l.FulfilledQuantity,
                    Cancelled = l.CancelledQuantity,
                    Remaining = Math.Max(0m,
                        l.RequestedQuantity - l.FulfilledQuantity - l.CancelledQuantity)
                }).ToList(),
                Fulfillments = r.Fulfillments
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new FulfillmentRow
                    {
                        ClaimId = f.ClaimId,
                        ShopName = NameFor(f.FulfillingShopId),
                        Status = f.Status,
                        TotalQuantity = f.Lines.Sum(l => l.Quantity),
                        TransferId = f.TransferId,
                        CreatedAt = f.CreatedAt,
                        ShippedAt = f.ShippedAt,
                        IsStuck = f.Status == StockRequestFulfillmentStatuses.Claimed
                                  && f.CreatedAt < stuckCutoff
                    }).ToList(),
                HasStuckClaim = r.Fulfillments.Any(f =>
                    f.Status == StockRequestFulfillmentStatuses.Claimed
                    && f.CreatedAt < stuckCutoff)
            }).ToList();

            // Filter dropdown options — every shop that ever requested.
            var requesterIds = await _context.SharedStockRequests
                .Select(r => r.RequestingShopId)
                .Distinct()
                .ToListAsync();
            ShopOptions = requesterIds.Select(id => (id, NameFor(id))).ToList();
        }

        // ─── Actions ────────────────────────────────────────────────────

        /// <summary>Deletes every closed (Completed/Cancelled) request — history cleanup.
        /// Lines + fulfillments cascade; the transfers themselves are independent
        /// documents and are untouched.</summary>
        public async Task<IActionResult> OnPostClearCompletedAsync()
        {
            var closed = await _context.SharedStockRequests
                .Where(r => r.Status != StockRequestStatuses.Open)
                .ToListAsync();
            _context.SharedStockRequests.RemoveRange(closed);
            await _context.SaveChangesAsync();

            _logger.LogInformation("StockRequests admin: cleared {Count} closed request(s).", closed.Count);
            Message = $"Διαγράφηκαν {closed.Count} ολοκληρωμένα/ακυρωμένα αιτήματα.";
            return RedirectToPage(new { StatusFilter, ShopFilter });
        }

        /// <summary>Deletes ONE closed request.</summary>
        public async Task<IActionResult> OnPostDeleteRequestAsync(Guid id)
        {
            var request = await _context.SharedStockRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null) return RedirectToPage(new { StatusFilter, ShopFilter });
            if (request.Status == StockRequestStatuses.Open)
            {
                Message = "Ανοιχτό αίτημα δεν διαγράφεται — ακυρώστε πρώτα το υπόλοιπο.";
                return RedirectToPage(new { StatusFilter, ShopFilter });
            }

            _context.SharedStockRequests.Remove(request);
            await _context.SaveChangesAsync();
            Message = "Το αίτημα διαγράφηκε.";
            return RedirectToPage(new { StatusFilter, ShopFilter });
        }

        /// <summary>Admin-side cancel of a request's unfulfilled remainder — identical
        /// semantics to the shop-facing cancelremainder endpoint.</summary>
        public async Task<IActionResult> OnPostCancelRemainderAsync(Guid id)
        {
            await using var tx = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var request = await _context.SharedStockRequests
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.RequestId == id);
            if (request == null || request.Status != StockRequestStatuses.Open)
            {
                Message = "Το αίτημα δεν είναι ανοιχτό.";
                return RedirectToPage(new { StatusFilter, ShopFilter });
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

            _logger.LogInformation("StockRequests admin: remainder cancelled on {RequestId} (now {Status}).",
                id, request.Status);
            Message = "Το υπόλοιπο του αιτήματος ακυρώθηκε.";
            return RedirectToPage(new { StatusFilter, ShopFilter });
        }

        /// <summary>Voids a STUCK claim (Claimed, no transfer arrived): restores the
        /// request's remaining quantity, reopens an auto-completed request — identical
        /// semantics to the shop-facing release endpoint. Never touches Shipped claims
        /// (goods are on their way; the push may merely be delayed).</summary>
        public async Task<IActionResult> OnPostVoidClaimAsync(Guid claimId)
        {
            await using var tx = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var fulfillment = await _context.SharedStockRequestFulfillments
                .Include(f => f.Lines)
                .FirstOrDefaultAsync(f => f.ClaimId == claimId);
            if (fulfillment == null || fulfillment.Status != StockRequestFulfillmentStatuses.Claimed)
            {
                Message = "Η δέσμευση δεν είναι πλέον σε κατάσταση «Claimed».";
                return RedirectToPage(new { StatusFilter, ShopFilter });
            }

            var request = await _context.SharedStockRequests
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.RequestId == fulfillment.RequestId);

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

            _logger.LogInformation("StockRequests admin: voided stuck claim {ClaimId} (request {RequestId}).",
                claimId, fulfillment.RequestId);
            Message = "Η δέσμευση ακυρώθηκε και οι ποσότητες επιστράφηκαν στο αίτημα.";
            return RedirectToPage(new { StatusFilter, ShopFilter });
        }
    }
}
