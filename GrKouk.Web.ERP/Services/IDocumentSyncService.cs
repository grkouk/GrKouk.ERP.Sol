using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Sync;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IDocumentSyncService
{
    Task<ServiceResult> SyncAddBusinessBuyDocument(SyncBusinessBuyDocumentRequest request, Guid? syncSessionId = null);
    Task<ServiceResult> SyncAddDayCloseData(DayClosePayload request, Guid? syncSessionId = null);
}

public class SyncBusinessDocService : IDocumentSyncService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<SyncBusinessDocService> _logger;
    private readonly IDocumentTransactionService _docTransSrv;

    public SyncBusinessDocService(ApiDbContext context, ILogger<SyncBusinessDocService> logger,
        IDocumentTransactionService docTransSrv)
    {
        _context = context;
        _logger = logger;
        _docTransSrv = docTransSrv;
    }

    public async Task<ServiceResult> SyncAddBusinessBuyDocument(SyncBusinessBuyDocumentRequest request,
        Guid? syncSessionId = null)
    {
        #region Boiler Plate Code

        string mainEntityName = SyncEntityNames.SyncBuyDocument;
        // string syncEntityName = SyncEntityNames.SyncBuyDocument;
        _logger.LogInformation("SyncBusinessBuyDocumentRequest");
        int addedCount = 0;
        int failedToAddCount = 0;
        int updatedCount = 0;
        int failedToUpdateCount = 0;
        int deletedCount = 0;
        int failedToDeleteCount = 0;
        if (request == null)
        {
            return ServiceResult.Error("Empty request data", "BADREQUEST");
        }

        if (string.IsNullOrEmpty(request.CompanyCode))
        {
            return ServiceResult.Error("No Company Code", "BADREQUEST");
        }

        var companyId = await FindCompanyIdByCode(request.CompanyCode);
        ;
        if (companyId < 0)
        {
            return ServiceResult.Error("No Company Code", "BADREQUEST");
        }

        var syncId = syncSessionId ?? Guid.NewGuid();
        var syncSource = "MAUI Client"; // Source of the sync operation
        string syncMerchItemCode = string.Empty;
        ;
        int syncMerchitemId = 0;
        string paymentMethodCashCode = "Μετρητοίς";
        int paymentMethodCashId = 0;
        string paymentMethodPistosiCode = "Επι Πιστώσει";
        int paymentMethodPistosiId = 0;

        int docSeriesId = 0;
        int syncSupplierId = 0;
        string syncSupplierName;
        int paymentMethodId = 0;

        #endregion

        #region Find Doc series id

        (docSeriesId, syncMerchItemCode) =
            await FindDocSeriesIdForBusDocIdAndCompanyCode(request.BuyDocDefId, request.CompanyCode);
        switch (docSeriesId)
        {
            case -1:
                return ServiceResult.Error("Default Doc Series not found", "BADREQUEST");

            case -2:
                return ServiceResult.Error("Business Doc Id is not syncable (yet)", "BADREQUEST");
        }

        #endregion

        #region Get default Merch item id

        syncMerchitemId = await FindWarehouseItemIdByCode(syncMerchItemCode);
        if (syncMerchitemId < 0)
        {
            return ServiceResult.Error("Default Merch item not found", "BADREQUEST");
        }

        #endregion

        #region Payment Methods

        paymentMethodCashId = await FindPaymentMethodIdByName(paymentMethodCashCode);
        if (paymentMethodCashId < 0)
        {
            return ServiceResult.Error("Default Cash Payment not found", "BADREQUEST");
        }

        paymentMethodPistosiId = await FindPaymentMethodIdByName(paymentMethodPistosiCode);
        if (paymentMethodPistosiId < 0)
        {
            return ServiceResult.Error("Default Pistosi Payment not found", "BADREQUEST");
        }

        #endregion

        #region "Find Synced Supplier"

        (syncSupplierId, syncSupplierName) = await FindSyncedErpSupplierId(request.SupplierId, request.CompanyCode);
        if (syncSupplierId < 0)
        {
            return ServiceResult.Error("Sync Supplier not found", "BADREQUEST");
        }

        string etiologyMessage =
            $"Synced Buy Document {request.RefNumber} for {syncSupplierName} Transaction date {request.TransDate:dddd dd/MM/yyyy} Amount {request.TotalAmount:C2}";

        #endregion

        if (request.PayedAmount > 0)
        {
            paymentMethodId = paymentMethodCashId;
        }
        else
        {
            paymentMethodId = paymentMethodPistosiId;
        }

        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            BuyDocLineAjaxDto docLine = new BuyDocLineAjaxDto
            {
                WarehouseItemId = syncMerchitemId,
                TransactionUnitId = 1,
                TransactionQuantity = 1,
                TransactionUnitFactor = 1,
                TransUnitPrice = request.TotalAmount,
                Q1 = 1,
                Q2 = 1,
                Price = request.TotalAmount,
                Amount = 0,
                AmountDiscount = 0,
                AmountExpenses = 0,
                DiscountRate = 0,
                MainUnitId = 1,
                SecUnitId = 1,
                Factor = 1,
                FpaRate = 0,
            };

            var docTrans = new BuyDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransactorId = syncSupplierId,
                BuyDocSeriesId = docSeriesId,
                TransRefCode = request.RefNumber.ToString(),
                AmountDiscount = 0,
                Etiology = etiologyMessage,
                PaymentMethodId = paymentMethodId,
                CompanyId = companyId,
                BuyDocLines = new List<BuyDocLineAjaxDto> { docLine },
                AmountFpa = request.VatAmount,
                AmountNet = request.NetAmount,
            };
            var result = await _docTransSrv.AddBuyDocument(docTrans);
            if (result is BadRequestObjectResult badRequestResult)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return ServiceResult.Error(badRequestResult.Value.ToString(), "BADREQUEST");
                
            }

            int newDocId = 0;
            if (result is OkObjectResult okResult)
            {
                newDocId = (int)okResult.Value;
            }

            var newSyncEntity = new SyncBuyDocument()
            {
                Id = Guid.NewGuid(),
                BusId = request.Id,
                ErpId = newDocId,
                BuyDocDefId = request.BuyDocDefId,
                BuyDocDefName = request.BuyDocDefName,
                TransDate = request.TransDate,
                RefNumber = request.RefNumber,
                CompanyCode = request.CompanyCode,
                SupplierId = request.SupplierId,
                SupplierName = request.SupplierName,
                VatAmount = request.VatAmount,
                NetAmount = request.NetAmount,
                TotalAmount = request.TotalAmount,
                PayedAmount = request.PayedAmount,
                SourceChecksum = ChecksumHelper.CalculateChecksum(request.Id.ToString(),
                    request.BuyDocDefId.ToString(),
                    request.TransDate.ToString(CultureInfo.InvariantCulture),
                    request.RefNumber.ToString(),
                    request.CompanyCode,
                    request.SupplierId.ToString(),
                    request.VatAmount.ToString(CultureInfo.InvariantCulture),
                    request.NetAmount.ToString(CultureInfo.InvariantCulture),
                    request.TotalAmount.ToString(CultureInfo.InvariantCulture),
                    request.PayedAmount.ToString(CultureInfo.InvariantCulture))
            };
            _context.SyncBuyDocuments.Add(newSyncEntity);
            _context.SynchronizationLogs.Add(new SynchronizationLog
            {
                Id = Guid.NewGuid(),
                SyncSessionId = syncId,
                EntityName = mainEntityName,
                EntityId = newSyncEntity.Id,
                CompanyCode = newSyncEntity.CompanyCode,
                OperationType = "INSERT",
                Source = syncSource,
            });

            try
            { 
                await _context.SaveChangesAsync();
                if (ownsTransaction) await transaction.CommitAsync();
                // throw new Exception("Test");
                addedCount++;
            }
            catch (Exception ex)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
                failedToAddCount++;
                return ServiceResult.Error($"Error:{ex.Message}");
            }


            //await _context.SaveChangesAsync();
            //if (ownsTransaction) await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        var res = new ErpSynchronizationResponse<SyncBuyDocument>
        {
            Message = "Document synced successfully",
            AddedCount = addedCount,
            FailedToAddCount = failedToAddCount,
            UpdatedCount = updatedCount,
            FailedToUpdateCount = failedToUpdateCount,
            DeletedCount = deletedCount,
            FailedToDeleteCount = failedToDeleteCount,
            SyncSessionId = syncId,
            SyncSource = syncSource,
            // SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
        };
        return ServiceResult.Ok(res);
    }

    public async Task<ServiceResult> SyncAddDayCloseData(DayClosePayload request, Guid? syncSessionId = null)
    {
        string mainEntityName = SyncEntityNames.SyncDayCloseDate;
        const string docSeriesLianiniCode = "ΑΠΛΠ";
        const string docSeriesLianiniStarCode = "ΑΠΛΠSTAR";
        _logger.LogInformation("SyncDayCloseData");

        #region Check parameters

        if (request == null)
        {
            return ServiceResult.Error("Empty request data", "BADREQUEST");
        }

        if (string.IsNullOrEmpty(request.CompanyCode))
        {
            return ServiceResult.Error("No Company Code", "BADREQUEST");
        }

        var companyId = await FindCompanyIdByCode(request.CompanyCode);
        ;
        if (companyId < 0)
        {
            return ServiceResult.Error("No Company Code", "BADREQUEST");
        }

        #endregion

        #region Varialble Declarations

        var syncId = syncSessionId ?? Guid.NewGuid();
        var syncSource = "MAUI Client"; // Source of the sync operation
        string syncMerchItemCode = "SYNCMERCH";
        
        int syncMerchitemId = 0;
        string paymentMethodCashCode = "Μετρητοίς";
        int paymentMethodCashId = 0;
        string paymentMethodCardsCode = "NBG POS";
        int paymentMethodCardsId = 0;

        int docSeriesId = 0;
        string syncCustomerCode = "ΠΕΛΛΙΑΝ";
        int syncCustomerId = 0;
        string syncSupplierName;
        int paymentMethodId = 0;

        #endregion

        #region Find Doc series id

        var docSeriesLianiki =
            await _context.SellDocSeriesDefs.SingleOrDefaultAsync(p => p.Code == docSeriesLianiniCode);
        if (docSeriesLianiki == null)
        {
            return ServiceResult.Error("Default Doc Series not found", "BADREQUEST");
        }

        var docSeriesLianikiId = docSeriesLianiki.Id;
        var docSeriesLianikiStar =
            await _context.SellDocSeriesDefs.SingleOrDefaultAsync(p => p.Code == docSeriesLianiniStarCode);
        if (docSeriesLianikiStar == null)
        {
            return ServiceResult.Error("Default Start Doc Series not found", "BADREQUEST");
        }

        var docSeriesLianikiStarId = docSeriesLianikiStar.Id;

        #endregion

        #region Get default Merch item id

        syncMerchitemId = await FindWarehouseItemIdByCode(syncMerchItemCode);
        if (syncMerchitemId < 0)
        {
            return ServiceResult.Error("Default Merch item not found", "BADREQUEST");
        }

        #endregion
        #region Payment Methods

        paymentMethodCashId = await FindPaymentMethodIdByName(paymentMethodCashCode);
        if (paymentMethodCashId < 0)
        {
            return ServiceResult.Error("Default Cash Payment not found", "BADREQUEST");
        }

        paymentMethodCardsId = await FindPaymentMethodIdByName(paymentMethodCardsCode);
        if (paymentMethodCardsId < 0)
        {
            return ServiceResult.Error("Default Card Payment not found", "BADREQUEST");
        }

        #endregion

        #region "Find Synced Customer"

       var customer=await _context.Transactors
           .Include(p=>p.TransactorType)
           .SingleOrDefaultAsync(p=>p.Code==syncCustomerCode && p.TransactorType.Code=="SYS.CUSTOMER");
        if (customer is null)
        {
            return ServiceResult.Error("Sync Customer not found", "BADREQUEST");
        }
        syncCustomerId = customer.Id;
        string etiologyMessage =
            $"Synced Day Close for Z {request.ZNumber}  Transaction date {request.TransDate:dddd dd/MM/yyyy}";

        #endregion
        
        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            SellDocLineAjaxDto docLine = new SellDocLineAjaxDto
            {
                WarehouseItemId = syncMerchitemId,
                TransactionUnitId = 1,
                TransactionQuantity = 1,
                TransactionUnitFactor = 1,
                TransUnitPrice = request.TotalCash,
                Q1 = 1,
                Q2 = 1,
                Price = request.TotalCash,
                Amount = 0,
                AmountDiscount = 0,
                AmountExpenses = 0,
                DiscountRate = 0,
                MainUnitId = 1,
                SecUnitId = 1,
                Factor = 1,
                FpaRate = 0,
            };

            var docTrans = new SellDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransactorId = syncCustomerId,
                SellDocSeriesId = docSeriesLianikiId,
                TransRefCode = request.ZNumber.ToString(),
                AmountDiscount = 0,
                Etiology = etiologyMessage + " cash payment",
                PaymentMethodId = paymentMethodCashId,
                CompanyId = companyId,
                SalesChannelId = 1,
                SellDocLines = new List<SellDocLineAjaxDto> { docLine },
                AmountFpa = 0,
                AmountNet = request.TotalCash,
            };
            
            var result = await _docTransSrv.AddSalesDoc(docTrans);
            if (result is BadRequestObjectResult badRequestResult)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return ServiceResult.Error(badRequestResult.Value.ToString(), "BADREQUEST");
                
            }
            SellDocLineAjaxDto docLineCards = new SellDocLineAjaxDto
            {
                WarehouseItemId = syncMerchitemId,
                TransactionUnitId = 1,
                TransactionQuantity = 1,
                TransactionUnitFactor = 1,
                TransUnitPrice = request.TotalCards,
                Q1 = 1,
                Q2 = 1,
                Price = request.TotalCards,
                Amount = 0,
                AmountDiscount = 0,
                AmountExpenses = 0,
                DiscountRate = 0,
                MainUnitId = 1,
                SecUnitId = 1,
                Factor = 1,
                FpaRate = 0,
            };
            var docTransCards = new SellDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransactorId = syncCustomerId,
                SellDocSeriesId = docSeriesLianikiId,
                TransRefCode = request.ZNumber.ToString(),
                AmountDiscount = 0,
                Etiology = etiologyMessage + " card payment",
                PaymentMethodId = paymentMethodCardsId,
                CompanyId = companyId,
                SalesChannelId = 1,
                SellDocLines = new List<SellDocLineAjaxDto> { docLineCards },
                AmountFpa = 0,
                AmountNet = request.TotalCards,
            };
            
            var resultCards = await _docTransSrv.AddSalesDoc(docTransCards);
            if (resultCards is BadRequestObjectResult badCardsRequestResult)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return ServiceResult.Error(badCardsRequestResult.Value.ToString(), "BADREQUEST");
                
            }
            SellDocLineAjaxDto docLineStar = new SellDocLineAjaxDto
            {
                WarehouseItemId = syncMerchitemId,
                TransactionUnitId = 1,
                TransactionQuantity = 1,
                TransactionUnitFactor = 1,
                TransUnitPrice = request.TotalStar,
                Q1 = 1,
                Q2 = 1,
                Price = request.TotalStar,
                Amount = 0,
                AmountDiscount = 0,
                AmountExpenses = 0,
                DiscountRate = 0,
                MainUnitId = 1,
                SecUnitId = 1,
                Factor = 1,
                FpaRate = 0,
            };
            var docTransStar = new SellDocCreateAjaxDto
            {
                TransDate = request.TransDate,
                TransactorId = syncCustomerId,
                SellDocSeriesId = docSeriesLianikiStarId,
                TransRefCode = request.ZNumber.ToString(),
                AmountDiscount = 0,
                Etiology = etiologyMessage,
                PaymentMethodId = paymentMethodCashId,
                CompanyId = companyId,
                SalesChannelId = 1,
                SellDocLines = new List<SellDocLineAjaxDto> { docLineStar },
                AmountFpa = 0,
                AmountNet = request.TotalStar,
            };
            
            var resultStar = await _docTransSrv.AddSalesDoc(docTransStar);
            if (resultStar is BadRequestObjectResult badStarRequestResult)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                return ServiceResult.Error(badStarRequestResult.Value.ToString(), "BADREQUEST");
            }
            try
            { 
                await _context.SaveChangesAsync();
                if (ownsTransaction) await transaction.CommitAsync();
                // throw new Exception("Test");
            }
            catch (Exception ex)
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
                return ServiceResult.Error($"Error:{ex.Message}");
            }
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }

        var res = new DayCloseResponse()
        {
            IsSuccess = true,
            Message = "Κλείσιμο ημέρας ενημερώθηκε με επιτυχία"
        };
        return ServiceResult.Ok(res);
    }

    #region Helper Methods

    private async Task<(int docId, string merchCode)> FindDocSeriesIdForBusDocIdAndCompanyCode(int busBuyDocDefId,
        string companyCode)
    {
        //To use for ilika in timologio agoron
        const string syncMerchItemCode = "SYNCMERCH";
        //To use for ipiresia in timologio paroxis
        const string syncMerchYpiresiaItemCode = "ΥΠΕΙΚ";
        string merchItemCode = string.Empty;
        const int busDocTypeTimologioAgId = 9;
        const int busDocTypePistorikoEpId = 17;
        const int busDocTypeTimParYpiresionAgId = 11;
        const string docSeriesTimAgCode = "TIMDAAGSYNC";
        const string docSeriesPistotikoEpAgCode = "PISTIMAGSYNC";
        const string docSeriesTimParYpiresionikoAgCode = "ΤΜΠΑΡΑΓΣΥΓΧ";
        string docSeriesCode;
        switch (busBuyDocDefId)
        {
            case busDocTypeTimologioAgId:
                docSeriesCode = docSeriesTimAgCode;
                merchItemCode = syncMerchItemCode;
                break;
            case busDocTypePistorikoEpId:
                docSeriesCode = docSeriesPistotikoEpAgCode;
                merchItemCode = syncMerchItemCode;
                break;
            case busDocTypeTimParYpiresionAgId:
                docSeriesCode = docSeriesTimParYpiresionikoAgCode;
                merchItemCode = syncMerchYpiresiaItemCode;
                break;
            default:
                return (-2, string.Empty);
        }

        var docSeries = await _context.BuyDocSeriesDefs.SingleOrDefaultAsync(p => p.Code == docSeriesCode);
        if (docSeries == null)
        {
            return (-1, string.Empty);
        }

        return (docSeries.Id, merchItemCode);
    }

    private async Task<int> FindCompanyIdByCode(string companyCode)
    {
        var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
        if (company == null)
        {
            return -1;
        }

        int companyId = company.Id;
        return companyId;
    }

    private async Task<int> FindPaymentMethodIdByName(string paymentMethodName)
    {
        var paymentMethod = await _context.PaymentMethods.SingleOrDefaultAsync(p => p.Name == paymentMethodName);
        if (paymentMethod == null)
        {
            return -1;
        }

        return paymentMethod.Id;
    }

    private async Task<int> FindWarehouseItemIdByCode(string itemCode)
    {
        var item = await _context.WarehouseItems.SingleOrDefaultAsync(p => p.Code == itemCode);
        if (item == null)
        {
            return -1;
        }

        return item.Id;
    }

    private async Task<(int supplierId, string supplierName)> FindSyncedErpSupplierId(int businessSupplierId,
        string companyCode)
    {
        var syncSupplier =
            await _context.SyncSuppliers.SingleOrDefaultAsync(p =>
                p.BusId == businessSupplierId && p.CompanyCode == companyCode);
        ;
        if (syncSupplier == null)
        {
            return (-1, string.Empty);
        }

        return (syncSupplier.ErpId, syncSupplier.Name);
    }

    #endregion
}