using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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