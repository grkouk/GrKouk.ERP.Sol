using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashRegister;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CashierApiController : ControllerBase
{
    private const string PaymentSectionCode = "SCNPAYMENTS";
    private const string PaymentDocSeriesCode = "ΑΠΠΛΗΡ";

    private readonly ApiDbContext _context;
    private readonly ILogger<CashierApiController> _logger;
    private readonly ITransactorTransactionService _transService;

    public CashierApiController(
        ApiDbContext context,
        ILogger<CashierApiController> logger,
        ITransactorTransactionService transService)
    {
        _context = context;
        _logger = logger;
        _transService = transService;
    }
/// <summary>
/// I have not desided yet wheather we get the suppliers from the main table or we use the sync table
/// </summary>
/// <param name="companyCode"></param>
/// <returns></returns>
    [HttpGet("GetErpLookupSuppliers")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> GetErpSyncSuppliers(string companyCode)
    {
        if (string.IsNullOrEmpty(companyCode))
        {
            return BadRequest(new
            {
                error = "No Company Code"
            });
        }
       
        var items = await _context.SyncSuppliers.Where(p => p.CompanyCode == companyCode)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(items);
    }
    /// <summary>
    /// Supplier payments list for the cashier management view. Filters by company, the
    /// SCNPAYMENTS section, optional date range, and word-based search against supplier
    /// name (with keyboard transliteration), series name, tax number, and ref code.
    /// </summary>
    [HttpGet("GetErpSupplierPayments")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult> GetErpSupplierPayments(string companyCode, string searchText, DateTime? dateFrom, DateTime? dateTo)
    {
        if (string.IsNullOrEmpty(companyCode))
        {
            return BadRequest(new { error = "Company code is required" });
        }
        var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
        if (company == null)
        {
            return BadRequest(new { error = $"Company with code '{companyCode}' not found" });
        }
        var section = await _context.Sections.SingleOrDefaultAsync(p => p.Code == PaymentSectionCode);
        if (section == null)
        {
            return BadRequest(new { error = $"Payment section with code '{PaymentSectionCode}' not found" });
        }

        var companyId = company.Id;
        var sectionId = section.Id;

        var query = _context.TransactorTransactions
            .Where(t => t.CompanyId == companyId && t.SectionId == sectionId);

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.Date;
            query = query.Where(t => t.TransDate >= from);
        }
        if (dateTo.HasValue)
        {
            var to = dateTo.Value.Date;
            query = query.Where(t => t.TransDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim().ToLower();
            var terms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var parameter = Expression.Parameter(typeof(TransactorTransaction), "i");
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

            // Two-hop property access for navigation properties
            var transactorProp = Expression.Property(parameter, nameof(TransactorTransaction.Transactor));
            var nameProp = Expression.Property(transactorProp, nameof(Transactor.Name));
            var nameLower = Expression.Call(nameProp, toLowerMethod);

            // 1. Transactor.Name matches ALL terms (with keyboard transliteration per term)
            Expression nameExpr = null;
            foreach (var term in terms)
            {
                var transliterated = KeyboardTransliterator.Transliterate(term);
                var contains = Expression.Call(nameLower, containsMethod, Expression.Constant(term));

                Expression termExpr = contains;
                if (transliterated != term)
                {
                    var transContains = Expression.Call(nameLower, containsMethod, Expression.Constant(transliterated));
                    termExpr = Expression.OrElse(contains, transContains);
                }

                nameExpr = nameExpr == null ? termExpr : Expression.AndAlso(nameExpr, termExpr);
            }

            // 2. Other fields match FULL search string
            var searchConst = Expression.Constant(search);

            Expression AddNullableContains(Expression target, string propName)
            {
                var prop = Expression.Property(target, propName);
                var notNull = Expression.NotEqual(prop, Expression.Constant(null));
                var lower = Expression.Call(prop, toLowerMethod);
                var contains = Expression.Call(lower, containsMethod, searchConst);
                return Expression.AndAlso(notNull, contains);
            }

            // Document series name (two-hop)
            var seriesProp = Expression.Property(parameter, nameof(TransactorTransaction.TransTransactorDocSeries));
            var seriesNameProp = Expression.Property(seriesProp, nameof(TransTransactorDocSeriesDef.Name));
            var seriesNameLower = Expression.Call(seriesNameProp, toLowerMethod);
            Expression otherExpr = Expression.Call(seriesNameLower, containsMethod, searchConst);

            // Transactor.TaxNumber (nullable)
            otherExpr = Expression.OrElse(otherExpr, AddNullableContains(transactorProp, nameof(Transactor.TaxNumber)));

            // TransactorTransaction.TransRefCode (nullable)
            otherExpr = Expression.OrElse(otherExpr, AddNullableContains(parameter, nameof(TransactorTransaction.TransRefCode)));

            var finalExpr = nameExpr != null ? Expression.OrElse(nameExpr, otherExpr) : otherExpr;
            var lambda = Expression.Lambda<Func<TransactorTransaction, bool>>(finalExpr, parameter);

            query = query.Where(lambda);
        }

        var items = await query
            .OrderBy(p => p.TransDate)
            .Select(p => new SupplierPaymentListItemDto
            {
                Id = p.Id,
                TransDate = p.TransDate,
                TransactorId = p.TransactorId,
                TransactorName = p.Transactor.Name,
                TransactorCode = p.Transactor.Code,
                DocSeriesName = p.TransTransactorDocSeries.Name,
                DocTypeName = p.TransTransactorDocType.Name,
                TransRefCode = p.TransRefCode,
                AmountNet = p.AmountNet,
                AmountFpa = p.AmountFpa,
                AmountDiscount = p.AmountDiscount,
                AmountSum = p.AmountNet + p.AmountFpa - p.AmountDiscount,
                Etiology = p.Etiology,
                Timestamp = p.Timestamp
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Returns a single supplier payment shaped as the edit-view upsert DTO (includes FpaRate
    /// and DiscountRate). Used by the cashier edit view to populate fields when editing.
    /// </summary>
    [HttpGet("supplier-payments/{id:int}")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult<SupplierPaymentUpsertDto>> GetSupplierPayment(int id)
    {
        var entity = await _context.TransactorTransactions
            .AsNoTracking()
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null) return NotFound(new { error = $"Supplier payment {id} not found" });

        var dto = new SupplierPaymentUpsertDto
        {
            Id = entity.Id,
            CompanyCode = entity.Company?.Code ?? string.Empty,
            TransactorId = entity.TransactorId,
            CfAccountId = entity.CfAccountId,
            TransDate = entity.TransDate,
            TransRefCode = entity.TransRefCode,
            AmountNet = entity.AmountNet,
            AmountFpa = entity.AmountFpa,
            AmountDiscount = entity.AmountDiscount,
            FpaRate = entity.FpaRate,
            DiscountRate = entity.DiscountRate,
            Etiology = entity.Etiology,
            Timestamp = entity.Timestamp
        };
        return Ok(dto);
    }

    /// <summary>
    /// Returns cash-flow accounts available to the caller's company (plus those mapped to the
    /// system-wide ALL companies entry). Used by the cashier supplier-payment edit view.
    /// </summary>
    [HttpGet("cfaccounts")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult> GetCfAccounts(string companyCode)
    {
        if (string.IsNullOrEmpty(companyCode))
            return BadRequest(new { error = "Company code is required" });

        var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
        if (company == null)
            return BadRequest(new { error = $"Company with code '{companyCode}' not found" });

        var allCompaniesId = await FiltersHelper.GetAllCompaniesIdAsync(_context);

        var accounts = await _context.CashFlowAccountCompanyMappings
            .AsNoTracking()
            .Include(p => p.CashFlowAccount)
            .Where(p => p.CompanyId == company.Id || p.CompanyId == allCompaniesId)
            .Select(p => new CfAccountLookupDto
            {
                Id = p.CashFlowAccount.Id,
                Code = p.CashFlowAccount.Code,
                Name = p.CashFlowAccount.Name
            })
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(accounts);
    }

    /// <summary>
    /// Creates a supplier payment (TransactorTransaction) via ITransactorTransactionService
    /// with callerSectionCode always null. Doc series is resolved server-side by fixed code
    /// ΑΠΠΛΗΡ (client does not select a doc series). CFA is handled internally by the
    /// TransactorTransactionService.
    /// </summary>
    [HttpPost("supplier-payments")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> CreateSupplierPayment([FromBody] SupplierPaymentUpsertDto dto)
    {
        if (dto == null) return BadRequest(new { error = "Payload is required" });
        if (string.IsNullOrEmpty(dto.CompanyCode))
        {
            return BadRequest(new { error = "Company code is required" });
        }
        var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == dto.CompanyCode);
        if (company == null)
        {
            return BadRequest(new { error = $"Company with code '{dto.CompanyCode}' not found" });
        }
        var docSeriesId = await ResolvePaymentDocSeriesIdAsync(company.Id);
        if (docSeriesId <= 0)
        {
            return BadRequest(new { error = $"Supplier payment doc series '{PaymentDocSeriesCode}' not found for company '{dto.CompanyCode}'" });
        }

        var createDto = new TransactorTransCreateDto
        {
            TransDate = dto.TransDate,
            TransTransactorDocSeriesId = docSeriesId,
            TransactorId = dto.TransactorId,
            CfAccountId = dto.CfAccountId,
            TransRefCode = dto.TransRefCode,
            CompanyId = company.Id,
            AmountNet = dto.AmountNet,
            AmountFpa = dto.AmountFpa,
            AmountDiscount = dto.AmountDiscount,
            FpaRate = dto.FpaRate,
            DiscountRate = dto.DiscountRate,
            Etiology = dto.Etiology
        };

        var result = await _transService.AddTransactorTransaction(createDto, callerSectionCode: null);
        if (result == null)
        {
            return StatusCode(500, new { error = "Empty response from transactor service" });
        }
        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// Updates an existing supplier payment. Timestamp is included for future concurrency
    /// enforcement — current service impl does not check it yet.
    /// </summary>
    [HttpPut("supplier-payments/{id:int}")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> UpdateSupplierPayment(int id, [FromBody] SupplierPaymentUpsertDto dto)
    {
        if (dto == null) return BadRequest(new { error = "Payload is required" });
        if (dto.Id.HasValue && dto.Id.Value != id)
        {
            return BadRequest(new { error = "Route id does not match payload id" });
        }
        if (string.IsNullOrEmpty(dto.CompanyCode))
        {
            return BadRequest(new { error = "Company code is required" });
        }
        var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == dto.CompanyCode);
        if (company == null)
        {
            return BadRequest(new { error = $"Company with code '{dto.CompanyCode}' not found" });
        }
        var docSeriesId = await ResolvePaymentDocSeriesIdAsync(company.Id);
        if (docSeriesId <= 0)
        {
            return BadRequest(new { error = $"Supplier payment doc series '{PaymentDocSeriesCode}' not found for company '{dto.CompanyCode}'" });
        }

        var modifyDto = new TransactorTransModifyDto
        {
            Id = id,
            TransDate = dto.TransDate,
            TransTransactorDocSeriesId = docSeriesId,
            TransactorId = dto.TransactorId,
            CfAccountId = dto.CfAccountId,
            TransRefCode = dto.TransRefCode,
            CompanyId = company.Id,
            AmountNet = dto.AmountNet,
            AmountFpa = dto.AmountFpa,
            AmountDiscount = dto.AmountDiscount,
            FpaRate = dto.FpaRate,
            DiscountRate = dto.DiscountRate,
            Etiology = dto.Etiology,
            Timestamp = dto.Timestamp
        };

        var result = await _transService.ModifyTransactorTransaction(modifyDto, callerSectionCode: null);
        if (result == null)
        {
            return StatusCode(500, new { error = "Empty response from transactor service" });
        }
        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }
        return Ok(new { success = true });
    }

    [HttpDelete("supplier-payments/{id:int}")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<IActionResult> DeleteSupplierPayment(int id)
    {
        var result = await _transService.DeleteTransactorTransaction(id);
        if (result == null)
        {
            return StatusCode(500, new { error = "Empty response from transactor service" });
        }
        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }
        return NoContent();
    }

    /// <summary>
    /// Supplier ledger ("kartela") for the cashier app. Returns the opening balance over the
    /// pre-period and the in-period transactions (Debit/Credit raw — no running total). The
    /// client computes the running total so it can be re-evaluated on grid sort. EUR-only:
    /// no exchange-rate conversion is applied (cashier-side amounts are already in base
    /// currency).
    /// </summary>
    [HttpGet("GetErpSupplierLedger")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult<SupplierLedgerResponseDto>> GetErpSupplierLedger(
        string companyCodes, int transactorId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(companyCodes))
        {
            return BadRequest(new { error = "Company code is required" });
        }
        if (transactorId <= 0)
        {
            return BadRequest(new { error = "Transactor id is required" });
        }

        // Companies are branches of one business; the supplier sees a single account.
        // Pool all selected branches and present them as one ledger, tagging each row
        // with its branch for the Company column.
        var codes = companyCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        if (codes.Count == 0)
        {
            return BadRequest(new { error = "Company code is required" });
        }

        var companies = await _context.Companies
            .Where(c => codes.Contains(c.Code))
            .Select(c => new { c.Id, c.Code, c.Name })
            .ToListAsync(ct);
        var missing = codes.Where(c => companies.All(co => co.Code != c)).ToList();
        if (missing.Count > 0)
        {
            return BadRequest(new { error = $"Company with code '{string.Join(", ", missing)}' not found" });
        }

        var companyIds = companies.Select(c => c.Id).ToList();
        var companyById = companies.ToDictionary(c => c.Id, c => c);
        var fromDate = dateFrom.Date;
        var toDate = dateTo.Date;

        // Rows posted in the payment section are editable supplier payments; their transaction id
        // is the id used by the supplier-payments edit/delete endpoints. Resolve the section once
        // so we can flag those rows. A missing section just means no row is flagged as a payment.
        var paymentSectionId = await _context.Sections
            .Where(s => s.Code == PaymentSectionCode)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);

        // Pre-period rows (for the opening balance). Pull the small raw projection and
        // derive Debit/Credit from FinancialAction in memory — keeps the SQL trivial.
        var beforeRaw = await _context.TransactorTransactions
            .Where(t => t.TransactorId == transactorId
                        && companyIds.Contains(t.CompanyId)
                        && t.TransDate < fromDate)
            .Select(t => new
            {
                t.FinancialAction,
                t.TransNetAmount,
                t.TransFpaAmount,
                t.TransDiscountAmount
            })
            .ToListAsync(ct);

        decimal openingDebit = 0m;
        decimal openingCredit = 0m;
        foreach (var r in beforeRaw)
        {
            var total = r.TransNetAmount + r.TransFpaAmount - r.TransDiscountAmount;
            if (r.FinancialAction == FinActionsEnum.FinActionsEnumDebit ||
                r.FinancialAction == FinActionsEnum.FinActionsEnumNegativeDebit)
            {
                openingDebit += total;
            }
            else if (r.FinancialAction == FinActionsEnum.FinActionsEnumCredit ||
                     r.FinancialAction == FinActionsEnum.FinActionsEnumNegativeCredit)
            {
                openingCredit += total;
            }
        }

        // Supplier convention: positive balance means we owe the supplier.
        var openingBalance = openingCredit - openingDebit;

        // In-period rows
        var inPeriodRaw = await _context.TransactorTransactions
            .Where(t => t.TransactorId == transactorId
                        && companyIds.Contains(t.CompanyId)
                        && t.TransDate >= fromDate
                        && t.TransDate <= toDate)
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.TransDate,
                DocSeriesName = t.TransTransactorDocSeries.Name,
                t.TransRefCode,
                t.FinancialAction,
                t.TransNetAmount,
                t.TransFpaAmount,
                t.TransDiscountAmount,
                t.CompanyId,
                t.SectionId
            })
            .ToListAsync(ct);

        var rows = new List<SupplierLedgerRowDto>(inPeriodRaw.Count);
        foreach (var r in inPeriodRaw)
        {
            var total = r.TransNetAmount + r.TransFpaAmount - r.TransDiscountAmount;
            decimal debit = 0m;
            decimal credit = 0m;
            if (r.FinancialAction == FinActionsEnum.FinActionsEnumDebit ||
                r.FinancialAction == FinActionsEnum.FinActionsEnumNegativeDebit)
            {
                debit = total;
            }
            else if (r.FinancialAction == FinActionsEnum.FinActionsEnumCredit ||
                     r.FinancialAction == FinActionsEnum.FinActionsEnumNegativeCredit)
            {
                credit = total;
            }

            companyById.TryGetValue(r.CompanyId, out var co);
            rows.Add(new SupplierLedgerRowDto
            {
                Id = r.Id,
                TransDate = r.TransDate,
                DocSeriesName = r.DocSeriesName,
                TransRefCode = r.TransRefCode,
                Debit = debit,
                Credit = credit,
                CompanyCode = co?.Code ?? string.Empty,
                CompanyName = co?.Name ?? string.Empty,
                IsPayment = paymentSectionId.HasValue && r.SectionId == paymentSectionId.Value
            });
        }

        var response = new SupplierLedgerResponseDto
        {
            OpeningBalance = openingBalance,
            OpeningBalanceDate = fromDate.AddDays(-1),
            Rows = rows
        };
        return Ok(response);
    }

    /// <summary>
    /// Open-item payables ("what we owe and when") for a supplier, pooled across the
    /// selected branches and treated as ONE account (the supplier sees one customer that
    /// merely ships to several branches). Per payable invoice (BuyDocument) the due date
    /// is the invoice date plus the payment-method credit term (DaysOverdue). Because
    /// cash-register supplier payments are recorded on-account (not matched to a specific
    /// invoice), the open amount per invoice is inferred by FIFO: oldest invoice first,
    /// applying the pooled reductions (payments / credit notes). Current = due on/before
    /// asOf; Future = not yet due. Residual reconciles to the true combined ledger
    /// balance (opening balances / manual credits not tied to a BuyDocument). EUR-only.
    /// </summary>
    [HttpGet("GetErpSupplierOpenItems")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult<SupplierOpenItemsResponseDto>> GetErpSupplierOpenItems(
        string companyCodes, int transactorId, DateTime? asOf = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(companyCodes))
        {
            return BadRequest(new { error = "Company code is required" });
        }
        if (transactorId <= 0)
        {
            return BadRequest(new { error = "Transactor id is required" });
        }

        var codes = companyCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        if (codes.Count == 0)
        {
            return BadRequest(new { error = "Company code is required" });
        }

        var companies = await _context.Companies
            .Where(c => codes.Contains(c.Code))
            .Select(c => new { c.Id, c.Code, c.Name })
            .ToListAsync(ct);
        var missing = codes.Where(c => companies.All(co => co.Code != c)).ToList();
        if (missing.Count > 0)
        {
            return BadRequest(new { error = $"Company with code '{string.Join(", ", missing)}' not found" });
        }

        var companyIds = companies.Select(c => c.Id).ToList();
        var companyById = companies.ToDictionary(c => c.Id, c => c);
        var asOfDate = (asOf ?? DateTime.Today).Date;

        // Payable invoices = BuyDocuments whose doc type posts a CREDIT to the transactor
        // (increases payable). PaymentMethod is left-joined: DaysOverdue may be null when
        // the doc has no payment method, in which case the term falls back to 0 (due on
        // the invoice date) and the result is flagged Approximate.
        var invoicesRaw = await _context.BuyDocuments
            .Where(b => b.TransactorId == transactorId
                        && companyIds.Contains(b.CompanyId)
                        && b.TransDate <= asOfDate
                        && b.BuyDocType.TransTransactorDef != null
                        && (b.BuyDocType.TransTransactorDef.FinancialTransAction == FinActionsEnum.FinActionsEnumCredit
                            || b.BuyDocType.TransTransactorDef.FinancialTransAction == FinActionsEnum.FinActionsEnumNegativeCredit))
            .Select(b => new
            {
                b.Id,
                b.CompanyId,
                b.TransDate,
                b.TransRefCode,
                DocSeriesName = b.BuyDocSeries.Name,
                b.TransNetAmount,
                b.TransFpaAmount,
                b.TransDiscountAmount,
                DaysOverdue = (int?)b.PaymentMethod.DaysOverdue
            })
            .ToListAsync(ct);

        // Pooled transactions (all branches, up to asOf) for the combined true balance and
        // the FIFO reduction pool. Derive Debit/Credit exactly like the ledger so the
        // numbers reconcile.
        var transRaw = await _context.TransactorTransactions
            .Where(t => t.TransactorId == transactorId
                        && companyIds.Contains(t.CompanyId)
                        && t.TransDate <= asOfDate)
            .Select(t => new
            {
                t.FinancialAction,
                t.TransNetAmount,
                t.TransFpaAmount,
                t.TransDiscountAmount
            })
            .ToListAsync(ct);

        decimal totalDebit = 0m;
        decimal totalCredit = 0m;
        foreach (var t in transRaw)
        {
            var total = t.TransNetAmount + t.TransFpaAmount - t.TransDiscountAmount;
            if (t.FinancialAction == FinActionsEnum.FinActionsEnumDebit ||
                t.FinancialAction == FinActionsEnum.FinActionsEnumNegativeDebit)
            {
                totalDebit += total;
            }
            else if (t.FinancialAction == FinActionsEnum.FinActionsEnumCredit ||
                     t.FinancialAction == FinActionsEnum.FinActionsEnumNegativeCredit)
            {
                totalCredit += total;
            }
        }

        // Supplier convention: positive balance means we owe the supplier.
        var combinedBalance = totalCredit - totalDebit;

        var invoices = invoicesRaw
            .Select(b => new
            {
                b.Id,
                b.CompanyId,
                b.TransDate,
                b.TransRefCode,
                b.DocSeriesName,
                Gross = b.TransNetAmount + b.TransFpaAmount - b.TransDiscountAmount,
                DueDate = b.TransDate.Date.AddDays(b.DaysOverdue ?? 0),
                HasTerm = b.DaysOverdue.HasValue
            })
            .OrderBy(b => b.TransDate)
            .ThenBy(b => b.Id)
            .ToList();

        // FIFO-apply pooled reductions (payments + credit notes) to the oldest invoices.
        var pool = totalDebit;
        var approximate = false;
        var openItems = new List<SupplierOpenItemDto>();
        foreach (var inv in invoices)
        {
            if (!inv.HasTerm)
            {
                approximate = true;
            }

            var open = inv.Gross;
            if (pool > 0m)
            {
                var applied = Math.Min(pool, open);
                open -= applied;
                pool -= applied;
            }

            if (open <= 0.0001m)
            {
                continue;
            }

            companyById.TryGetValue(inv.CompanyId, out var co);
            openItems.Add(new SupplierOpenItemDto
            {
                BuyDocumentId = inv.Id,
                CompanyCode = co?.Code ?? string.Empty,
                CompanyName = co?.Name ?? string.Empty,
                DocDate = inv.TransDate,
                DueDate = inv.DueDate,
                DocSeriesName = inv.DocSeriesName,
                DocRef = inv.TransRefCode,
                OriginalAmount = inv.Gross,
                OpenAmount = open,
                DaysPastDue = (int)(asOfDate - inv.DueDate.Date).TotalDays
            });
        }

        var openSum = openItems.Sum(o => o.OpenAmount);
        var residual = combinedBalance - openSum;
        if (Math.Abs(residual) > 0.0001m)
        {
            approximate = true;
        }

        var current = openItems
            .Where(o => o.DueDate.Date <= asOfDate)
            .OrderBy(o => o.DueDate)
            .ThenBy(o => o.BuyDocumentId)
            .ToList();
        var future = openItems
            .Where(o => o.DueDate.Date > asOfDate)
            .OrderBy(o => o.DueDate)
            .ThenBy(o => o.BuyDocumentId)
            .ToList();

        return Ok(new SupplierOpenItemsResponseDto
        {
            Current = current,
            Future = future,
            CurrentTotal = current.Sum(o => o.OpenAmount),
            FutureTotal = future.Sum(o => o.OpenAmount),
            Residual = residual,
            CombinedBalance = combinedBalance,
            Approximate = approximate
        });
    }

    private async Task<int> ResolvePaymentDocSeriesIdAsync(int companyId)
    {
        var allCompaniesId = await FiltersHelper.GetAllCompaniesIdAsync(_context);
        return await _context.TransTransactorDocSeriesDefs
            .AsNoTracking()
            .Where(s => (s.CompanyId == companyId || s.CompanyId == allCompaniesId)
                        && s.Active
                        && s.Code == PaymentDocSeriesCode)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
    }
}