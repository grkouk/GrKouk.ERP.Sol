using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Controllers;
/// <summary>
/// This controller is not in use yet so dont call it
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CashierApiController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<CashierApiController> _logger;

    public CashierApiController(ApiDbContext context, ILogger<CashierApiController> logger)
    {
        _context = context;
        _logger = logger;
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
    /// This is for the client management view for working with the search text 
    /// </summary>
    /// <param name="companyCode"></param>
    /// <param name="searchText"></param>
    /// <param name="datePeriod"></param>
    /// <returns></returns>
    [HttpGet("GetErpSupplierPayments")]
    [Authorize(Policy = "ApiPolicy2")]
    public async Task<ActionResult> GetErpSupplierPayments(string companyCode, string searchText, string datePeriod="All")
    {
        var query = _context.TransactorTransactions
            .Include(p => p.Company)
            .Include(p => p.Section)
            .Include(p => p.Transactor)
            .Include(p => p.TransTransactorDocSeries)
            .Include(p => p.TransTransactorDocType)
            .AsQueryable();
        if (string.IsNullOrEmpty(companyCode))
        {
            return BadRequest(new 
            {
                error = "Company code is required"
            });
        }
        var company = _context.Companies.SingleOrDefault(p => p.Code == companyCode);
        if (company == null)
        {
           return BadRequest(new 
           {
               error = $"Company with code '{companyCode}' not found"
           });
        }
        var companyId = company.Id;
        string paymentSectionCode = "SCNPAYMENTS";
        var section = _context.Sections.SingleOrDefault(p => p.Code == paymentSectionCode);
        if (section == null)
        {
            return BadRequest(new 
            {
                error = $"Payment section with code '{paymentSectionCode}' not found"
            });
        }
        
        int[] sectionIds = [section.Id];

        //var (dateFrom, dateTo) = getDatePeriodFromDatePeriodString(datePeriod);
        // 🔹 Base filters (Company, Date, Section)
        query = query.Where(t =>
            t.CompanyId == companyId &&
            //t.Translate >= dateFrom &&
            //t.Translate <= dateTo &&
            sectionIds.Contains(t.SectionId)
        );
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim().ToLower();
            var terms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Build Expression tree to replicate "Words search like ItemSearchDialog"
            // Logic: 
            // (Name contains term1 AND Name contains term2 ...)
            // OR (Code contains fullSearch)
            // OR (Ean/Upc/Manufacturer contains fullSearch)
            // OR (Codes.Any contains fullSearch)

            var parameter = Expression.Parameter(typeof(TransactorTransaction), "i");

            // 1. Name matches ALL terms
            var nameProp = Expression.Property(parameter, nameof(TransactorTransaction.Transactor.Name));
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

            Expression? nameExpr = null;
            var nameLower = Expression.Call(nameProp, toLowerMethod);

            foreach (var term in terms)
            {
                var termConst = Expression.Constant(term);
                var contains = Expression.Call(nameLower, containsMethod, termConst);
                nameExpr = nameExpr == null ? contains : Expression.AndAlso(nameExpr, contains);
            }

            // 2. Other fields match FULL search string
            var searchConst = Expression.Constant(search);

            // Document series name
            var codeProp = Expression.Property(parameter, nameof(TransactorTransaction.TransTransactorDocSeries.Name));
            var codeLower = Expression.Call(codeProp, toLowerMethod);
            var codeContains = Expression.Call(codeLower, containsMethod, searchConst);

            Expression otherExpr = codeContains;

            // // EanCode (Nullable)
            // Expression AddNullableContains(string propName)
            // {
            //     var prop = Expression.Property(parameter, propName);
            //     var notNull = Expression.NotEqual(prop, Expression.Constant(null));
            //     var lower = Expression.Call(prop, toLowerMethod);
            //     var contains = Expression.Call(lower, containsMethod, searchConst);
            //     return Expression.AndAlso(notNull, contains);
            // }
            //
            // otherExpr = Expression.OrElse(otherExpr, AddNullableContains(nameof(Item.EanCode)));
            // otherExpr = Expression.OrElse(otherExpr, AddNullableContains(nameof(Item.UpcCode)));
            // otherExpr = Expression.OrElse(otherExpr, AddNullableContains(nameof(Item.ManufacturerCode)));

            // Codes Collection Any()


            // Final Combined Expression: (NameMatches OR OtherMatches)
            var finalExpr = nameExpr != null ? Expression.OrElse(nameExpr, otherExpr) : otherExpr;
            var lambda = Expression.Lambda<Func<TransactorTransaction, bool>>(finalExpr, parameter);

            query = query.Where(lambda);
        }

        query = query.OrderBy(p => p.TransDate);
        var items = await query
            // .Select(p => new 
            //     { p.Id, p.TransDate, p.TransactorId, p.TransactorName, p.Amount, p.Description })
            .ToListAsync();
        return Ok(items);
    }
}