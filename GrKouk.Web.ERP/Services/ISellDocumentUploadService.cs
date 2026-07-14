using System;
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

public interface ISellDocumentUploadService
{
    /// <summary>
    /// Uploads a cash-register per-document "ledger sell" (e.g. supplier bonus credit) to the
    /// ERP as a sales document. Idempotent on LocalSellDocumentId. If ExistingErpSellDocId is
    /// set, deletes that ERP doc first (delete-and-replace). Returns the ERP SellDocument.Id in
    /// Data on success.
    /// </summary>
    Task<ServiceResult> UploadAsync(SellDocumentUploadRequest request);

    /// <summary>
    /// Deletes an ERP SellDocument previously uploaded by the cash register
    /// (tombstone-driven deletion). Idempotent: missing rows return Ok.
    /// </summary>
    Task<ServiceResult> DeleteAsync(Guid localSellDocumentId, int erpSellDocId, string companyCode);
}

public class SellDocumentUploadService : ISellDocumentUploadService
{
    private readonly ApiDbContext _context;
    private readonly IDocumentTransactionService _docTransSrv;
    private readonly ILogger<SellDocumentUploadService> _logger;

    public SellDocumentUploadService(
        ApiDbContext context,
        IDocumentTransactionService docTransSrv,
        ILogger<SellDocumentUploadService> logger)
    {
        _context = context;
        _docTransSrv = docTransSrv;
        _logger = logger;
    }

    public async Task<ServiceResult> UploadAsync(SellDocumentUploadRequest request)
    {
        if (request == null)
            return ServiceResult.Error("Request is null", "BADREQUEST");
        if (request.Lines == null || request.Lines.Count == 0)
            return ServiceResult.Error("Upload must include at least one line", "BADREQUEST");

        var existing = await _context.UploadedSellDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.LocalSellDocumentId == request.LocalSellDocumentId);

        if (existing != null && request.ExistingErpSellDocId == null)
        {
            return ServiceResult.Ok(existing.ErpSellDocId);
        }

        var company = await _context.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == request.CompanyCode);
        if (company == null)
            return ServiceResult.Error($"Company with code {request.CompanyCode} not found", "COMPANY_NOT_FOUND");
        int companyId = company.Id;

        // SellDocument.SalesChannel is a required FK on the ERP side, but the cash register has
        // no sales-channel concept (sends 0). Resolve a default channel so the insert succeeds.
        int salesChannelId = request.SalesChannelId;
        if (salesChannelId <= 0)
        {
            salesChannelId = await _context.SalesChannels
                .OrderBy(c => c.Name)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
            if (salesChannelId <= 0)
                return ServiceResult.Error("No SalesChannel configured on the ERP", "NO_SALES_CHANNEL");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (request.ExistingErpSellDocId.HasValue)
            {
                var delResult = await _docTransSrv.DeleteSaleDocument(request.ExistingErpSellDocId.Value);
                if (!delResult.Success)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.Error(
                        $"Failed to delete existing ERP SellDoc {request.ExistingErpSellDocId.Value}: {delResult.ErrorMessage}",
                        delResult.ErrorCode ?? "DELETE_FAILED");
                }
            }

            var docTrans = new SellDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransRefCode = request.TransRefCode ?? string.Empty,
                TransactorId = request.TransactorId,
                SellDocSeriesId = request.SellDocSeriesId,
                PaymentMethodId = request.PaymentMethodId,
                SalesChannelId = salesChannelId,
                CompanyId = companyId,
                AmountNet = request.AmountNet,
                AmountFpa = request.AmountFpa,
                AmountDiscount = request.AmountDiscount,
                Etiology = request.Etiology ?? string.Empty,
                SellDocLines = request.Lines.Select(l => new SellDocLineAjaxDto
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

            var result = await _docTransSrv.AddSalesDoc(docTrans);
            if (result is BadRequestObjectResult bad)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error(bad.Value?.ToString() ?? "AddSalesDoc failed", "BADREQUEST");
            }
            if (result is NotFoundObjectResult nf)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error(nf.Value?.ToString() ?? "AddSalesDoc not found", "NOT_FOUND");
            }
            int newDocId = 0;
            if (result is OkObjectResult ok && ok.Value is int id)
                newDocId = id;
            if (newDocId <= 0)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error("AddSalesDoc did not return a doc id", "NO_DOC_ID");
            }

            var tracking = existing != null
                ? await _context.UploadedSellDocuments.FirstOrDefaultAsync(p => p.Id == existing.Id)
                : null;
            if (tracking != null)
            {
                tracking.ErpSellDocId = newDocId;
                tracking.CompanyCode = request.CompanyCode;
                tracking.UploadedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UploadedSellDocuments.Add(new UploadedSellDocument
                {
                    Id = Guid.NewGuid(),
                    LocalSellDocumentId = request.LocalSellDocumentId,
                    ErpSellDocId = newDocId,
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
            _logger.LogError(ex, "Upload SellDocument failed for local id {LocalId}", request.LocalSellDocumentId);
            try { await transaction.RollbackAsync(); } catch { }
            return ServiceResult.Error(ex.Message, "EXCEPTION");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid localSellDocumentId, int erpSellDocId, string companyCode)
    {
        var tracking = await _context.UploadedSellDocuments
            .FirstOrDefaultAsync(p => p.LocalSellDocumentId == localSellDocumentId);

        if (tracking == null)
        {
            _logger.LogInformation("Delete SellDocument: no tracking row for local id {LocalId} — treating as already deleted",
                localSellDocumentId);
            return ServiceResult.Ok();
        }

        if (tracking.ErpSellDocId != erpSellDocId)
        {
            return ServiceResult.Error(
                $"Tombstone ErpSellDocId {erpSellDocId} does not match tracking row {tracking.ErpSellDocId}",
                "ID_MISMATCH");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var delResult = await _docTransSrv.DeleteSaleDocument(erpSellDocId);
            if (!delResult.Success)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Error(
                    $"Failed to delete ERP SellDoc {erpSellDocId}: {delResult.ErrorMessage}",
                    delResult.ErrorCode ?? "DELETE_FAILED");
            }

            _context.UploadedSellDocuments.Remove(tracking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete SellDocument failed for local id {LocalId}", localSellDocumentId);
            try { await transaction.RollbackAsync(); } catch { }
            return ServiceResult.Error(ex.Message, "EXCEPTION");
        }
    }
}
