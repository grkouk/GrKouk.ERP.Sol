using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashRegister;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrKouk.Web.ERP.Services;

/// <summary>
/// The shared myDATA reconciliation memory: each cash register publishes its answers about the myDATA
/// documents, and every cash register reads all shops' answers plus the ERP's checks of its own copies.
/// The ERP stores answers only. It holds no feed and never talks to AADE.
/// </summary>
public interface IMyDataClaimsService
{
    /// <summary>Replaces the company's answers for the period. Data = <see cref="MyDataPublishResultDto"/>.</summary>
    Task<ServiceResult> PublishAsync(MyDataPublishRequestDto request, CancellationToken ct = default);

    /// <summary>All the companies' answers and the ERP's checks. Data = <see cref="MyDataClaimsResponseDto"/>.</summary>
    Task<ServiceResult> GetClaimsAsync(string companyCodes, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);

    /// <summary>Sets one issuer's rule. The action Normal removes it. Data = the stored ΑΦΜ key.</summary>
    Task<ServiceResult> SetIssuerRuleAsync(MyDataIssuerRuleDto rule, CancellationToken ct = default);

    /// <summary>
    /// Purchases per supplier, pooled across the companies. Data = a list of
    /// <see cref="MyDataErpSupplierTotalDto"/>. Read only.
    /// </summary>
    Task<ServiceResult> GetSupplierTotalsAsync(string companyCodes, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
}

public class MyDataClaimsService : IMyDataClaimsService
{
    /// <summary>
    /// ERP copy vs shop, gross. Measured on 983 uploaded documents: 927 agree within this, and the other
    /// 56 are real errors (the discount subtracted twice before the 2026-09-25 upload fix).
    /// </summary>
    private const decimal GrossTolerance = 0.02m;

    /// <summary>
    /// ERP copy vs shop, per VAT rate. The ERP recalculates every line (price x quantity, then the rate)
    /// instead of keeping the shop's amounts, so 35 of 1,223 rates drift by 2 to 10 cents with nothing
    /// wrong. Same as the shops' own per-rate tolerance. Only 2 rates were over it, both real.
    /// </summary>
    private const decimal RateTolerance = 0.10m;

    /// <summary>How far from the myDATA issue date an ERP-only document may be.</summary>
    private const int ErpOnlyMaxDays = 60;

    /// <summary>Scale of the rate in BuyDocLine.FpaRate, decimal(18,2).</summary>
    private const int RateScale = 2;

    private readonly ApiDbContext _context;
    private readonly ILogger<MyDataClaimsService> _logger;

    public MyDataClaimsService(ApiDbContext context, ILogger<MyDataClaimsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult> PublishAsync(MyDataPublishRequestDto request, CancellationToken ct = default)
    {
        if (request?.Claims == null)
            return ServiceResult.Error("The request has no answer list", "BADREQUEST");
        if (string.IsNullOrWhiteSpace(request.CompanyCode))
            return ServiceResult.Error("Company code is required", "BADREQUEST");

        var from = request.DateFrom.Date;
        var to = request.DateTo.Date;
        if (to < from)
            return ServiceResult.Error("The period ends before it starts", "BADREQUEST");
        var toExclusive = to.AddDays(1);

        var companyCode = request.CompanyCode.Trim();
        if (!await _context.Companies.AsNoTracking().AnyAsync(c => c.Code == companyCode, ct))
            return ServiceResult.Error($"Company with code '{companyCode}' not found", "COMPANY_NOT_FOUND");

        // Refused whole, before anything is deleted: a list that is only partly valid must not replace a
        // good one. A document outside the period would also survive the next replace of its own period.
        var problem = FindClaimProblem(request.Claims, from, toExclusive);
        if (problem != null)
            return ServiceResult.Error(problem, "BADREQUEST");

        var now = DateTime.UtcNow;
        var rows = request.Claims.Select(c => ToEntity(c, companyCode, now)).ToList();

        await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            await _context.MyDataShopClaims
                .Where(c => c.CompanyCode == companyCode && c.IssueDate >= from && c.IssueDate < toExclusive)
                .ExecuteDeleteAsync(ct);

            _context.MyDataShopClaims.AddRange(rows);

            var publish = await _context.MyDataShopPublishes.FirstOrDefaultAsync(
                p => p.CompanyCode == companyCode && p.PeriodFrom == from && p.PeriodTo == to, ct);
            if (publish == null)
            {
                publish = new MyDataShopPublish { CompanyCode = companyCode, PeriodFrom = from, PeriodTo = to };
                _context.MyDataShopPublishes.Add(publish);
            }
            publish.PublishedAt = now;
            publish.ClientVersion = request.ClientVersion;
            publish.Count = rows.Count;

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            // Mostly two publishes of one shop at the same moment. Nothing was kept, and the next
            // publish sends the full list again, so a caller can simply retry.
            _logger.LogWarning(ex, "myDATA publish failed for company {CompanyCode}", companyCode);
            await tx.RollbackAsync(CancellationToken.None);
            _context.ChangeTracker.Clear();
            return ServiceResult.Error("The publish could not be saved, please try again", "PUBLISH_CONFLICT");
        }

        return ServiceResult.Ok(new MyDataPublishResultDto { PublishedAt = now, Count = rows.Count });
    }

    public async Task<ServiceResult> GetClaimsAsync(
        string companyCodes, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        var codes = (companyCodes ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        if (codes.Count == 0)
            return ServiceResult.Error("Company code is required", "BADREQUEST");

        var from = dateFrom.Date;
        var to = dateTo.Date;
        if (to < from)
            return ServiceResult.Error("The period ends before it starts", "BADREQUEST");
        var toExclusive = to.AddDays(1);

        var companies = await _context.Companies.AsNoTracking()
            .Where(c => codes.Contains(c.Code))
            .Select(c => new { c.Id, c.Code })
            .ToListAsync(ct);
        var missing = codes.Where(c => companies.All(co => co.Code != c)).ToList();
        if (missing.Count > 0)
            return ServiceResult.Error($"Company with code '{string.Join(", ", missing)}' not found", "COMPANY_NOT_FOUND");

        var claims = await _context.MyDataShopClaims.AsNoTracking()
            .Where(c => codes.Contains(c.CompanyCode) && c.IssueDate >= from && c.IssueDate < toExclusive)
            .OrderBy(c => c.IssueDate).ThenBy(c => c.Mark).ThenBy(c => c.CompanyCode)
            .ToListAsync(ct);

        var response = new MyDataClaimsResponseDto
        {
            Claims = claims.Select(ToStoredDto).ToList(),
            Publishes = await _context.MyDataShopPublishes.AsNoTracking()
                .Where(p => codes.Contains(p.CompanyCode) && p.PeriodFrom <= to && p.PeriodTo >= from)
                .OrderBy(p => p.CompanyCode).ThenByDescending(p => p.PublishedAt)
                .Select(p => new MyDataPublishInfoDto
                {
                    CompanyCode = p.CompanyCode,
                    PeriodFrom = p.PeriodFrom,
                    PeriodTo = p.PeriodTo,
                    PublishedAt = p.PublishedAt,
                    ClientVersion = p.ClientVersion,
                    Count = p.Count
                })
                .ToListAsync(ct),
            IssuerRules = await _context.MyDataIssuerRules.AsNoTracking()
                .OrderBy(r => r.IssuerVat)
                .Select(r => new MyDataIssuerRuleDto
                {
                    IssuerVat = r.IssuerVat,
                    Action = r.Action,
                    Note = r.Note,
                    By = r.By,
                    At = r.At
                })
                .ToListAsync(ct)
        };

        await CheckErpCopiesAsync(response.Claims, ct);
        response.ErpOnlyChecks = await CheckErpOnlyAsync(response.Claims, response.IssuerRules, ct);

        var unpublished = companies
            .Where(c => !response.Publishes.Any(p => p.CompanyCode == c.Code && p.PeriodFrom <= from && p.PeriodTo >= to))
            .ToList();
        if (unpublished.Count > 0)
        {
            var ids = unpublished.Select(c => c.Id).ToList();
            var codeById = unpublished.ToDictionary(c => c.Id, c => c.Code);
            var headers = await _context.BuyDocuments.AsNoTracking()
                .Where(b => ids.Contains(b.CompanyId) && b.TransDate >= from && b.TransDate < toExclusive)
                .OrderBy(b => b.TransDate).ThenBy(b => b.Id)
                .Select(b => new
                {
                    b.Id,
                    b.CompanyId,
                    b.TransDate,
                    b.TransRefCode,
                    b.Transactor.TaxNumber,
                    Gross = b.AmountNet + b.AmountFpa - b.AmountDiscount
                })
                .ToListAsync(ct);
            response.FallbackHeaders = headers.Select(h => new MyDataErpHeaderDto
            {
                ErpBuyDocId = h.Id,
                CompanyCode = codeById[h.CompanyId],
                Date = h.TransDate,
                RefCode = h.TransRefCode,
                SupplierVat = h.TaxNumber,
                Gross = h.Gross
            }).ToList();
        }

        return ServiceResult.Ok(response);
    }

    public async Task<ServiceResult> SetIssuerRuleAsync(MyDataIssuerRuleDto rule, CancellationToken ct = default)
    {
        if (rule == null)
            return ServiceResult.Error("The request has no rule", "BADREQUEST");
        var vat = NormalizeVat(rule.IssuerVat);
        if (vat.Length == 0)
            return ServiceResult.Error("Issuer ΑΦΜ is required", "BADREQUEST");
        if (!MyDataIssuerActions.IsValid(rule.Action))
            return ServiceResult.Error($"Unknown action '{rule.Action}'", "BADREQUEST");

        var existing = await _context.MyDataIssuerRules.FirstOrDefaultAsync(r => r.IssuerVat == vat, ct);
        if (rule.Action == MyDataIssuerActions.Normal)
        {
            if (existing != null)
                _context.MyDataIssuerRules.Remove(existing);
        }
        else
        {
            if (existing == null)
            {
                existing = new MyDataIssuerRule { IssuerVat = vat };
                _context.MyDataIssuerRules.Add(existing);
            }
            existing.Action = rule.Action;
            existing.Note = rule.Note;
            existing.By = rule.By;
            existing.At = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return ServiceResult.Ok(vat);
    }

    public async Task<ServiceResult> GetSupplierTotalsAsync(
        string companyCodes, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        var codes = (companyCodes ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        if (codes.Count == 0)
            return ServiceResult.Error("Company code is required", "BADREQUEST");

        var from = dateFrom.Date;
        var to = dateTo.Date;
        if (to < from)
            return ServiceResult.Error("The period ends before it starts", "BADREQUEST");
        var toExclusive = to.AddDays(1);

        var companies = await _context.Companies.AsNoTracking()
            .Where(c => codes.Contains(c.Code))
            .Select(c => new { c.Id, c.Code })
            .ToListAsync(ct);
        var missing = codes.Where(c => companies.All(co => co.Code != c)).ToList();
        if (missing.Count > 0)
            return ServiceResult.Error($"Company with code '{string.Join(", ", missing)}' not found", "COMPANY_NOT_FOUND");
        var companyIds = companies.Select(c => c.Id).ToList();

        var docs = await _context.BuyDocuments.AsNoTracking()
            .Where(b => companyIds.Contains(b.CompanyId) && b.TransDate >= from && b.TransDate < toExclusive)
            .Select(b => new
            {
                b.TransactorId,
                b.Transactor.TaxNumber,
                b.Transactor.Name,
                b.TransRefCode,
                b.TransDate,
                Action = b.BuyDocType.TransTransactorDef.FinancialTransAction,
                Gross = b.AmountNet + b.AmountFpa - b.AmountDiscount
            })
            .ToListAsync(ct);

        var totals = docs
            .Where(d => d.Action != FinActionsEnum.FinActionsEnumNoChange)
            .GroupBy(d => new { d.TransactorId, d.TaxNumber, d.Name })
            .Select(g => new MyDataErpSupplierTotalDto
            {
                TransactorId = g.Key.TransactorId,
                TaxNumber = g.Key.TaxNumber,
                Name = g.Key.Name,
                Documents = g.Count(),
                Gross = g.Sum(d => SupplierSign(d.Action) * d.Gross),
                Docs = g.Select(d => new MyDataErpSupplierDocDto { RefCode = d.TransRefCode, Date = d.TransDate, Gross = d.Gross }).ToList()
            })
            .OrderBy(t => t.Name)
            .ToList();
        return ServiceResult.Ok(totals);
    }

    /// <summary>What a document does to the amount owed: an invoice adds, a credit note takes away.</summary>
    private static decimal SupplierSign(FinActionsEnum action) =>
        action is FinActionsEnum.FinActionsEnumCredit or FinActionsEnum.FinActionsEnumNegativeDebit ? 1m : -1m;

    private static string FindClaimProblem(List<MyDataClaimDto> claims, DateTime from, DateTime toExclusive)
    {
        var marks = new HashSet<long>();
        foreach (var claim in claims)
        {
            if (claim == null)
                return "The answer list has an empty row";
            if (claim.Mark <= 0)
                return $"Invalid mark {claim.Mark}";
            if (!marks.Add(claim.Mark))
                return $"Mark {claim.Mark} appears twice";
            if (!MyDataClaimKinds.IsValid(claim.ClaimKind))
                return $"Mark {claim.Mark}: unknown answer '{claim.ClaimKind}'";
            if (claim.IssueDate < from || claim.IssueDate >= toExclusive)
                return $"Mark {claim.Mark}: issue date {claim.IssueDate:yyyy-MM-dd} is outside the period";
        }
        return null;
    }

    private static MyDataShopClaim ToEntity(MyDataClaimDto c, string companyCode, DateTime publishedAt) => new()
    {
        CompanyCode = companyCode,
        Mark = c.Mark,
        ClaimKind = c.ClaimKind,
        IssuerVat = c.IssuerVat,
        Series = c.Series,
        Aa = c.Aa,
        IssueDate = c.IssueDate,
        MyDataGross = c.MyDataGross,
        ReconStatus = c.ReconStatus,
        ReconCauses = c.ReconCauses,
        LocalBuyDocumentId = c.LocalBuyDocumentId,
        LocalRefNumber = c.LocalRefNumber,
        LocalDate = c.LocalDate,
        LocalGross = c.LocalGross,
        LocalRatesJson = c.LocalRates == null ? null : JsonConvert.SerializeObject(c.LocalRates),
        LocalContentModifiedAt = c.LocalContentModifiedAt,
        UploadState = c.UploadState,
        ErpBuyDocId = c.ErpBuyDocId,
        AddressConflict = c.AddressConflict,
        AcceptedNote = c.AcceptedNote,
        AcceptedBy = c.AcceptedBy,
        AcceptedAt = c.AcceptedAt,
        PublishedAt = publishedAt
    };

    private static MyDataStoredClaimDto ToStoredDto(MyDataShopClaim c) => new()
    {
        CompanyCode = c.CompanyCode,
        PublishedAt = c.PublishedAt,
        Mark = c.Mark,
        ClaimKind = c.ClaimKind,
        IssuerVat = c.IssuerVat,
        Series = c.Series,
        Aa = c.Aa,
        IssueDate = c.IssueDate,
        MyDataGross = c.MyDataGross,
        ReconStatus = c.ReconStatus,
        ReconCauses = c.ReconCauses,
        LocalBuyDocumentId = c.LocalBuyDocumentId,
        LocalRefNumber = c.LocalRefNumber,
        LocalDate = c.LocalDate,
        LocalGross = c.LocalGross,
        LocalRates = string.IsNullOrEmpty(c.LocalRatesJson)
            ? null
            : JsonConvert.DeserializeObject<List<MyDataClaimRateDto>>(c.LocalRatesJson),
        LocalContentModifiedAt = c.LocalContentModifiedAt,
        UploadState = c.UploadState,
        ErpBuyDocId = c.ErpBuyDocId,
        AddressConflict = c.AddressConflict,
        AcceptedNote = c.AcceptedNote,
        AcceptedBy = c.AcceptedBy,
        AcceptedAt = c.AcceptedAt
    };

    /// <summary>
    /// ERP copy vs shop, for every Entered answer. A document the shop does not know it uploaded is found
    /// through the ERP's own upload record, so it reads as uploaded, not as NotUploaded.
    /// </summary>
    private async Task CheckErpCopiesAsync(List<MyDataStoredClaimDto> claims, CancellationToken ct)
    {
        var entered = claims.Where(c => c.ClaimKind == MyDataClaimKinds.Entered).ToList();
        if (entered.Count == 0) return;

        var localIds = entered.Where(c => c.ErpBuyDocId == null && c.LocalBuyDocumentId != null)
            .Select(c => c.LocalBuyDocumentId.Value)
            .ToList();
        var uploaded = localIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _context.UploadedBuyDocuments.AsNoTracking()
                .Where(u => localIds.Contains(u.LocalBuyDocumentId))
                .ToDictionaryAsync(u => u.LocalBuyDocumentId, u => u.ErpBuyDocId, ct);

        foreach (var claim in entered)
        {
            claim.ErpCheckedBuyDocId = claim.ErpBuyDocId
                ?? (claim.LocalBuyDocumentId is { } id && uploaded.TryGetValue(id, out var erpId) ? erpId : null);
        }

        var docIds = entered.Where(c => c.ErpCheckedBuyDocId != null).Select(c => c.ErpCheckedBuyDocId.Value)
            .Distinct()
            .ToList();
        var docs = docIds.Count == 0
            ? new()
            : await _context.BuyDocuments.AsNoTracking()
                .Where(b => docIds.Contains(b.Id))
                .Select(b => new
                {
                    b.Id,
                    CompanyCode = b.Company.Code,
                    Gross = b.AmountNet + b.AmountFpa - b.AmountDiscount
                })
                .ToDictionaryAsync(b => b.Id, ct);
        var lines = docIds.Count == 0
            ? new()
            : await _context.BuyDocLines.AsNoTracking()
                .Where(l => docIds.Contains(l.BuyDocumentId))
                .Select(l => new { l.BuyDocumentId, l.FpaRate, Net = l.AmountNet - l.AmountDiscount, Vat = l.AmountFpa })
                .ToListAsync(ct);
        var ratesByDoc = lines
            .GroupBy(l => l.BuyDocumentId)
            .ToDictionary(g => g.Key, g => g
                .GroupBy(l => Round(l.FpaRate, RateScale))
                .Select(r => new MyDataClaimRateDto { Rate = r.Key, Net = r.Sum(x => x.Net), Vat = r.Sum(x => x.Vat) })
                .OrderByDescending(r => r.Rate)
                .ToList());

        foreach (var claim in entered)
        {
            if (claim.ErpCheckedBuyDocId is not { } docId)
            {
                claim.ErpCheck = MyDataErpCheckResults.NotUploaded;
                continue;
            }
            if (!docs.TryGetValue(docId, out var doc))
            {
                claim.ErpCheck = MyDataErpCheckResults.Missing;
                continue;
            }

            claim.ErpCompanyCode = doc.CompanyCode;
            claim.ErpGross = doc.Gross;
            claim.ErpRates = ratesByDoc.GetValueOrDefault(docId) ?? new List<MyDataClaimRateDto>();
            claim.ErpCheck = doc.CompanyCode == claim.CompanyCode
                             && Near(claim.LocalGross ?? 0m, doc.Gross, GrossTolerance)
                             && RatesAgree(claim.LocalRates, claim.ErpRates)
                ? MyDataErpCheckResults.Ok
                : MyDataErpCheckResults.Differs;
        }
    }

    private static bool RatesAgree(List<MyDataClaimRateDto> shop, List<MyDataClaimRateDto> erp)
    {
        var shopByRate = (shop ?? new List<MyDataClaimRateDto>())
            .GroupBy(r => Round(r.Rate, RateScale))
            .ToDictionary(g => g.Key, g => (Net: g.Sum(x => x.Net), Vat: g.Sum(x => x.Vat)));
        var erpByRate = erp.ToDictionary(r => r.Rate, r => (Net: r.Net, Vat: r.Vat));

        return shopByRate.Keys.Union(erpByRate.Keys).All(rate =>
        {
            var s = shopByRate.GetValueOrDefault(rate);
            var e = erpByRate.GetValueOrDefault(rate);
            return Near(s.Net, e.Net, RateTolerance) && Near(s.Vat, e.Vat, RateTolerance);
        });
    }

    /// <summary>
    /// Finds the documents that a shop marked ERP-only, or whose issuer rule is ErpOnly, in ANY ERP
    /// company: same supplier ΑΦΜ, same document-number digits, near the same date. Same number rule as
    /// the cash register's own matching.
    /// </summary>
    private async Task<List<MyDataErpOnlyCheckDto>> CheckErpOnlyAsync(
        List<MyDataStoredClaimDto> claims, List<MyDataIssuerRuleDto> rules, CancellationToken ct)
    {
        var erpOnlyVats = rules.Where(r => r.Action == MyDataIssuerActions.ErpOnly)
            .Select(r => r.IssuerVat)
            .ToHashSet();
        var wanted = claims
            .Where(c => c.ClaimKind == MyDataClaimKinds.ErpOnly
                        || (c.ClaimKind != MyDataClaimKinds.Entered && erpOnlyVats.Contains(NormalizeVat(c.IssuerVat))))
            .GroupBy(c => c.Mark)
            .Select(g => g.First())
            .ToList();
        if (wanted.Count == 0) return new List<MyDataErpOnlyCheckDto>();

        var wantedVats = wanted.Select(c => NormalizeVat(c.IssuerVat)).ToHashSet();
        var transactors = await _context.Transactors.AsNoTracking()
            .Where(t => t.TaxNumber != null && t.TaxNumber != string.Empty)
            .Select(t => new { t.Id, t.TaxNumber })
            .ToListAsync(ct);
        var transactorsByVat = transactors
            .Select(t => new { t.Id, Vat = NormalizeVat(t.TaxNumber) })
            .Where(t => wantedVats.Contains(t.Vat))
            .ToLookup(t => t.Vat, t => t.Id);

        var transactorIds = transactorsByVat.SelectMany(g => g).Distinct().ToList();
        var earliest = wanted.Min(c => c.IssueDate).Date.AddDays(-ErpOnlyMaxDays);
        var latest = wanted.Max(c => c.IssueDate).Date.AddDays(ErpOnlyMaxDays + 1);
        var candidates = transactorIds.Count == 0
            ? new()
            : await _context.BuyDocuments.AsNoTracking()
                .Where(b => transactorIds.Contains(b.TransactorId) && b.TransDate >= earliest && b.TransDate < latest)
                .Select(b => new
                {
                    b.Id,
                    b.TransactorId,
                    b.TransRefCode,
                    b.TransDate,
                    CompanyCode = b.Company.Code,
                    Gross = b.AmountNet + b.AmountFpa - b.AmountDiscount
                })
                .ToListAsync(ct);
        var candidatesByTransactor = candidates
            .Select(b => new { Doc = b, Digits = Digits(b.TransRefCode) })
            .Where(b => b.Digits.Length > 0)
            .ToLookup(b => b.Doc.TransactorId);

        var results = new List<MyDataErpOnlyCheckDto>(wanted.Count);
        foreach (var claim in wanted)
        {
            var ids = transactorsByVat[NormalizeVat(claim.IssuerVat)].ToList();
            if (ids.Count == 0)
            {
                results.Add(new MyDataErpOnlyCheckDto { Mark = claim.Mark, Result = MyDataErpOnlyResults.NoSupplier });
                continue;
            }

            var aa = Digits(claim.Aa);
            var hit = aa.Length == 0
                ? null
                : ids.SelectMany(id => candidatesByTransactor[id])
                    .Where(b => (b.Digits == aa || (aa.Length >= 4 && b.Digits.EndsWith(aa, StringComparison.Ordinal)))
                                && Math.Abs((b.Doc.TransDate.Date - claim.IssueDate.Date).TotalDays) <= ErpOnlyMaxDays)
                    .OrderBy(b => Math.Abs(b.Doc.Gross - claim.MyDataGross))
                    .ThenBy(b => Math.Abs((b.Doc.TransDate.Date - claim.IssueDate.Date).TotalDays))
                    .FirstOrDefault();

            results.Add(hit == null
                ? new MyDataErpOnlyCheckDto { Mark = claim.Mark, Result = MyDataErpOnlyResults.NotFound }
                : new MyDataErpOnlyCheckDto
                {
                    Mark = claim.Mark,
                    Result = MyDataErpOnlyResults.Found,
                    ErpBuyDocId = hit.Doc.Id,
                    CompanyCode = hit.Doc.CompanyCode,
                    RefCode = hit.Doc.TransRefCode,
                    Date = hit.Doc.TransDate,
                    Gross = hit.Doc.Gross
                });
        }
        return results;
    }

    /// <summary>Compared at cents, so that a difference in the 3rd or 4th decimal alone never counts.</summary>
    private static bool Near(decimal a, decimal b, decimal tolerance) =>
        Math.Abs(Round(a, 2) - Round(b, 2)) <= tolerance;

    private static decimal Round(decimal value, int scale) =>
        Math.Round(value, scale, MidpointRounding.AwayFromZero);

    private static string Digits(string text) =>
        string.IsNullOrEmpty(text) ? string.Empty : new string(text.Where(char.IsDigit).ToArray());

    /// <summary>ΑΦΜ comparison key: digits only, no EL/GR prefix, no leading zeros. Same as the cash register.</summary>
    private static string NormalizeVat(string vat) => Digits(vat).TrimStart('0');
}
