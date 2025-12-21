using System;
using System.Text.Json;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IWarehouseManagementService
{
    Task<ServiceResult> AddWarehouseItemAsync(WarehouseItemCreateDto item);
    Task<ServiceResult> ModifyWarehouseItemAsync(WarehouseItemModifyDto item);
    Task<ServiceResult> DeleteWarehouseItemAsync(int itemId);
}

public class WarehouseManagementService : IWarehouseManagementService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<WarehouseManagementService> _logger;

    public WarehouseManagementService(ApiDbContext context, ILogger<WarehouseManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult> AddWarehouseItemAsync(WarehouseItemCreateDto item)
    {
        _logger.LogInformation("Adding warehouse item: {Item}", item);
        if (item is null)
        {
            return ServiceResult.Error("Δεν υπάρχει είδος προς εισαγωγή", "BADREQUEST");
        }
        WarehouseItem newEntity;
        // Check if a transaction is already active
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            var warehouseItem = new WarehouseItem
            {
                Active = item.Active,
                Name = item.Name,
                ShortDescription = item.Name,
                Description = item.Description,
                Code = item.Code,
                BuyMeasureUnitId = (int)item.BuyMeasureUnitId,
                MainMeasureUnitId = (int)item.MainMeasureUnitId,
                SecondaryMeasureUnitId = (int)item.SecondaryMeasureUnitId,
                BuyUnitToMainRate = item.BuyUnitToMainRate,
                SecondaryUnitToMainRate = item.SecondaryUnitToMainRate,
                FpaDefId = item.FpaDefId,
                MaterialCategoryId = item.MaterialCategoryId,
                MaterialType = item.MaterialType,
                ManufacturerCode = item.ManufacturerCode,
                BarCode = item.BarCode,
                PriceNetto = item.PriceNetto,
                PriceBrutto = item.PriceBrutto,

            };
           
            await _context.WarehouseItems.AddAsync(warehouseItem);
            await _context.SaveChangesAsync();
            string[] companiesSelected = JsonSerializer.Deserialize<string[]>(item.SelectedCompanies);
            foreach (var i in companiesSelected)
            {
                if (!Int32.TryParse(i, out int compId))
                {
                    throw new Exception("Selected company Id error");
                }
                warehouseItem.CompanyMappings.Add(new CompanyWarehouseItemMapping
                {
                    CompanyId = compId,
                    WarehouseItemId = warehouseItem.Id
                });
            }
            newEntity = _context.Entry(warehouseItem).Entity;

        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            return ServiceResult.Error($"Error:{ex.Message}", "BADREQUEST");
        }
        finally
        {
            // Dispose the transaction only if we created it
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
        
        return ServiceResult.Ok(newEntity );
    }

    public async Task<ServiceResult> ModifyWarehouseItemAsync(WarehouseItemModifyDto item)
    {
        _logger.LogInformation("Modifying warehouse item: {Item}", item);
        if (item is null || item.Id <= 0)
        {
            return ServiceResult.Error("Δεν υπάρχει είδος προς ενημέρωση", "BADREQUEST");
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteWarehouseItemAsync(int itemId)
    {
        _logger.LogInformation("Deleting warehouse item: {ItemId}", itemId);
        if (itemId <= 0)
        {
            return ServiceResult.Error("Δεν υπάρχει είδος προς διαγραφή", "BADREQUEST");
        }
        throw new System.NotImplementedException();
    }
}