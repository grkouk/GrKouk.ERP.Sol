using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Sync;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IBuyDocumentUploadService
{
    /// <summary>
    /// Uploads a cash-register BuyDocument to the ERP. Idempotent on LocalBuyDocumentId.
    /// If ExistingErpBuyDocId is set, deletes that ERP doc first (delete-and-replace).
    /// Returns the ERP BuyDocument.Id in Data on success.
    /// </summary>
    Task<ServiceResult> UploadAsync(BuyDocumentUploadRequest request);

    /// <summary>
    /// Deletes an ERP BuyDocument previously uploaded by the cash register
    /// (tombstone-driven deletion). Idempotent: missing rows return Ok.
    /// </summary>
    Task<ServiceResult> DeleteAsync(Guid localBuyDocumentId, int erpBuyDocId, string companyCode);
}

public class BuyDocumentUploadService : IBuyDocumentUploadService
{
    private readonly ApiDbContext _context;
    private readonly IDocumentTransactionService _docTransSrv;
    private readonly ILogger<BuyDocumentUploadService> _logger;

    public BuyDocumentUploadService(
        ApiDbContext context,
        IDocumentTransactionService docTransSrv,
        ILogger<BuyDocumentUploadService> logger)
    {
        _context = context;
        _docTransSrv = docTransSrv;
        _logger = logger;
    }

    public async Task<ServiceResult> UploadAsync(BuyDocumentUploadRequest request)
    {
        if (request == null)
            return ServiceResult.Error("Request is null", "BADREQUEST");
        if (request.Lines == null || request.Lines.Count == 0)
            return ServiceResult.Error("Upload must include at least one line", "BADREQUEST");

        var existing = await _context.UploadedBuyDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.LocalBuyDocumentId == request.LocalBuyDocumentId);

        if (existing != null && request.ExistingErpBuyDocId == null)
        {
            return ServiceResult.Ok(existing.ErpBuyDocId);
        }

        var company = await _context.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == request.CompanyCode);
        if (company == null)
            return ServiceResult.Error($"Company with code {request.CompanyCode} not found", "COMPANY_NOT_FOUND");
        int companyId = company.Id;

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (request.ExistingErpBuyDocId.HasValue)
            {
                var delResult = await _docTransSrv.DeleteBuyDocument(request.ExistingErpBuyDocId.Value);
                if (!delResult.Success)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Error(
                        $"Failed to delete existing ERP BuyDoc {request.ExistingErpBuyDocId.Value}: {delResult.ErrorMessage}",
                        delResult.ErrorCode ?? "DELETE_FAILED");
                }
            }

            var docTrans = new BuyDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransRefCode = request.TransRefCode ?? string.Empty,
                TransactorId = request.TransactorId,
                BuyDocSeriesId = request.BuyDocSeriesId,
                PaymentMethodId = request.PaymentMethodId,
                CompanyId = companyId,
                AmountNet = request.AmountNet,
                AmountFpa = request.AmountFpa,
                AmountDiscount = request.AmountDiscount,
                Etiology = request.Etiology ?? string.Empty,
                BuyDocLines = request.Lines.Select(l => new BuyDocLineAjaxDto
                {
                    WarehouseItemId = l.WarehouseItemId,
                    MainUnitId = l.MainUnitId,
                    SecUnitId = l.SecUnitId,
                    TransactionUnitId = l.TransactionUnitId,
                    TransactionUnitFactor = l.TransactionUnitFactor,
                    Factor = l.Factor,
                    TransactionQuantity = l.TransactionQuantity,
                    Q1 = l.Q1,
                    Q2 = l.Q2,
                    TransUnitPrice = l.TransUnitPrice,
                    Price = l.Price,
                    Amount = l.Amount,
                    AmountDiscount = l.AmountDiscount,
                    AmountExpenses = l.AmountExpenses,
                    DiscountRate = l.DiscountRate,
                    FpaRate = l.FpaRate
                }).ToList()
            };

            var result = await _docTransSrv.AddBuyDocument(docTrans);
            if (result is BadRequestObjectResult bad)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error(bad.Value?.ToString() ?? "AddBuyDocument failed", "BADREQUEST");
            }
            int newDocId = 0;
            if (result is OkObjectResult ok && ok.Value is int id)
                newDocId = id;
            if (newDocId <= 0)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error("AddBuyDocument did not return a doc id", "NO_DOC_ID");
            }

            var tracking = existing != null
                ? await _context.UploadedBuyDocuments.FirstOrDefaultAsync(p => p.Id == existing.Id)
                : null;
            if (tracking != null)
            {
                tracking.ErpBuyDocId = newDocId;
                tracking.CompanyCode = request.CompanyCode;
                tracking.UploadedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UploadedBuyDocuments.Add(new UploadedBuyDocument
                {
                    Id = Guid.NewGuid(),
                    LocalBuyDocumentId = request.LocalBuyDocumentId,
                    ErpBuyDocId = newDocId,
                    CompanyCode = request.CompanyCode,
                    UploadedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult.Ok(newDocId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload BuyDocument failed for local id {LocalId}", request.LocalBuyDocumentId);
            try { await transaction.RollbackAsync(); } catch { }
            return ServiceResult.Error(ex.Message, "EXCEPTION");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid localBuyDocumentId, int erpBuyDocId, string companyCode)
    {
        var tracking = await _context.UploadedBuyDocuments
            .FirstOrDefaultAsync(p => p.LocalBuyDocumentId == localBuyDocumentId);

        if (tracking == null)
        {
            _logger.LogInformation("Delete BuyDocument: no tracking row for local id {LocalId} — treating as already deleted",
                localBuyDocumentId);
            return ServiceResult.Ok();
        }

        if (tracking.ErpBuyDocId != erpBuyDocId)
        {
            return ServiceResult.Error(
                $"Tombstone ErpBuyDocId {erpBuyDocId} does not match tracking row {tracking.ErpBuyDocId}",
                "ID_MISMATCH");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var delResult = await _docTransSrv.DeleteBuyDocument(erpBuyDocId);
            if (!delResult.Success)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error(
                    $"Failed to delete ERP BuyDoc {erpBuyDocId}: {delResult.ErrorMessage}",
                    delResult.ErrorCode ?? "DELETE_FAILED");
            }

            _context.UploadedBuyDocuments.Remove(tracking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete BuyDocument failed for local id {LocalId}", localBuyDocumentId);
            try { await transaction.RollbackAsync(); } catch { }
            return ServiceResult.Error(ex.Message, "EXCEPTION");
        }
    }
}
