using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Sync;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IDayCloseUploadService
{
    /// <summary>
    /// Uploads a cash-register V2 day-close (one or more bucket SellDocuments)
    /// to the ERP. Idempotent on SubmissionId — a retry returns the original
    /// ErpSellDocIds instead of double-booking.
    /// </summary>
    Task<ServiceResult> UploadAsync(DayCloseUploadRequest request);
}

public class DayCloseUploadService : IDayCloseUploadService
{
    private const string RetailCustomerCode = "ΠΕΛΛΙΑΝ";
    private const string RetailCustomerTypeCode = "SYS.CUSTOMER";
    private const int DefaultSalesChannelId = 1;
    private const int DefaultUnitId = 1;

    private readonly ApiDbContext _context;
    private readonly IDocumentTransactionService _docTransSrv;
    private readonly ILogger<DayCloseUploadService> _logger;

    public DayCloseUploadService(
        ApiDbContext context,
        IDocumentTransactionService docTransSrv,
        ILogger<DayCloseUploadService> logger)
    {
        _context = context;
        _docTransSrv = docTransSrv;
        _logger = logger;
    }

    public async Task<ServiceResult> UploadAsync(DayCloseUploadRequest request)
    {
        if (request == null)
            return ServiceResult.Error("Request is null", "BADREQUEST");
        if (request.SubmissionId == Guid.Empty)
            return ServiceResult.Error("SubmissionId is required", "BADREQUEST");
        if (string.IsNullOrWhiteSpace(request.CompanyCode))
            return ServiceResult.Error("CompanyCode is required", "BADREQUEST");
        if (request.Documents == null || request.Documents.Count == 0)
            return ServiceResult.Error("At least one bucket document is required", "BADREQUEST");

        // Idempotency replay — return existing ids if this SubmissionId was already processed.
        var prior = await _context.UploadedDayCloseSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SubmissionId == request.SubmissionId);
        if (prior != null)
        {
            return ServiceResult.Ok(new DayCloseUploadResponse
            {
                SubmissionId = prior.SubmissionId,
                ErpSellDocIds = ParseIdsCsv(prior.ErpSellDocIdsCsv)
            });
        }

        var company = await _context.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == request.CompanyCode);
        if (company == null)
            return ServiceResult.Error(
                $"Company with code {request.CompanyCode} not found", "COMPANY_NOT_FOUND");
        int companyId = company.Id;

        var customer = await _context.Transactors
            .Include(p => p.TransactorType)
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == RetailCustomerCode && p.TransactorType.Code == RetailCustomerTypeCode);
        if (customer == null)
            return ServiceResult.Error(
                $"Retail customer ({RetailCustomerCode}) not found", "CUSTOMER_NOT_FOUND");
        int retailCustomerId = customer.Id;

        // Per-document self-consistency check (Net+Vat=Brut), independent of client claims.
        for (int i = 0; i < request.Documents.Count; i++)
        {
            var doc = request.Documents[i];
            if (doc.Lines == null || doc.Lines.Count == 0)
                return ServiceResult.Error(
                    $"Document #{i} has no lines", "BADREQUEST");

            foreach (var line in doc.Lines)
            {
                if (line.AmountNet + line.AmountFpa != line.Amount)
                    return ServiceResult.Error(
                        $"Document #{i} item {line.WarehouseItemId}: Net+Vat≠Brut", "BADREQUEST");
            }
            var sumNet = doc.Lines.Sum(l => l.AmountNet);
            var sumVat = doc.Lines.Sum(l => l.AmountFpa);
            var sumBrut = doc.Lines.Sum(l => l.Amount);
            if (sumNet != doc.AmountNet || sumVat != doc.AmountFpa || sumBrut != doc.AmountBrut)
                return ServiceResult.Error(
                    $"Document #{i}: header totals do not match line sums", "BADREQUEST");
        }

        var etiologyBase =
            $"Day Close V2 Z {request.ZNumber} {request.TransDate:dd/MM/yyyy}";

        var createdIds = new List<int>(request.Documents.Count);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            for (int i = 0; i < request.Documents.Count; i++)
            {
                var doc = request.Documents[i];
                var marker = doc.PaymentKind switch
                {
                    "star" => "ST",
                    "cash" => "CH",
                    "card" => "CD",
                    _ => doc.IsStar ? "ST" : (i + 1).ToString()
                };
                var docTrans = new SellDocCreateAjaxDto
                {
                    TransDate = request.TransDate.Date,
                    TransactorId = retailCustomerId,
                    SellDocSeriesId = doc.ErpSellDocSeriesId,
                    PaymentMethodId = doc.ErpPaymentMethodId,
                    CompanyId = companyId,
                    SalesChannelId = DefaultSalesChannelId,
                    TransRefCode = $"Z{request.ZNumber}-{marker}",
                    Etiology = $"{etiologyBase} (bucket {i + 1}/{request.Documents.Count}{(doc.IsStar ? ", star" : "")})",
                    AmountNet = doc.AmountNet,
                    AmountFpa = doc.AmountFpa,
                    AmountDiscount = 0,
                    SellDocLines = doc.Lines.Select(l => new SellDocLineAjaxDto
                    {
                        WarehouseItemId = l.WarehouseItemId,
                        TransactionUnitId = DefaultUnitId,
                        MainUnitId = DefaultUnitId,
                        SecUnitId = DefaultUnitId,
                        TransactionUnitFactor = 1f,
                        Factor = 1f,
                        TransactionQuantity = l.Quantity,
                        Q1 = l.Quantity,
                        Q2 = l.Quantity,
                        TransUnitPrice = l.AmountNet,
                        Price = l.AmountNet,
                        Amount = l.AmountNet,
                        AmountDiscount = 0,
                        AmountExpenses = 0,
                        DiscountRate = 0,
                        FpaRate = l.FpaRate
                    }).ToList()
                };

                var result = await _docTransSrv.AddSalesDoc(docTrans);
                if (result is BadRequestObjectResult bad)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Error(
                        $"AddSalesDoc failed for bucket {i + 1}: {bad.Value}", "ADD_SELL_DOC_FAILED");
                }
                if (result is NotFoundObjectResult nf)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Error(
                        $"AddSalesDoc not-found for bucket {i + 1}: {nf.Value}", "ADD_SELL_DOC_NOT_FOUND");
                }
                int newDocId = 0;
                if (result is OkObjectResult ok && ok.Value is int id)
                    newDocId = id;
                if (newDocId <= 0)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Error(
                        $"AddSalesDoc returned no id for bucket {i + 1}", "NO_DOC_ID");
                }
                createdIds.Add(newDocId);
            }

            _context.UploadedDayCloseSubmissions.Add(new UploadedDayCloseSubmission
            {
                Id = Guid.NewGuid(),
                SubmissionId = request.SubmissionId,
                CompanyCode = request.CompanyCode,
                ZNumber = request.ZNumber,
                ErpSellDocIdsCsv = string.Join(",", createdIds),
                UploadedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Ok(new DayCloseUploadResponse
            {
                SubmissionId = request.SubmissionId,
                ErpSellDocIds = createdIds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Day close V2 upload failed for submission {SubmissionId}", request.SubmissionId);
            try { await transaction.RollbackAsync(); } catch { }
            return ServiceResult.Error(ex.Message, "EXCEPTION");
        }
    }

    private static List<int> ParseIdsCsv(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<int>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var v) ? v : 0)
            .Where(v => v > 0)
            .ToList();
    }
}
