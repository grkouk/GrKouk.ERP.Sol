using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrKouk.Web.ERP.Services;

public interface IWarehouseManagementSrv
{
    Task<ServiceResult> AddWarehouseItemAsync(WarehouseItemCreateDto item);
    Task<ServiceResult> ModifyWarehouseItemAsync(WarehouseItemModifyDto item);
    Task<ServiceResult> DeleteWarehouseItemAsync(int itemId);
    Task<ServiceResult> AddCompanyMappingToWarehouseItemAsync(int warehouseItemId, int companyId);
}

public class WarehouseManagementSrv : IWarehouseManagementSrv
{
    private readonly ApiDbContext _context;
    private readonly ILogger<WarehouseManagementSrv> _logger;

    public WarehouseManagementSrv(ApiDbContext context, ILogger<WarehouseManagementSrv> logger)
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
                ShortDescription = item.ShortDescription,
                Description = item.Description,
                Code = item.Code,
                BuyMeasureUnitId = item.BuyMeasureUnitId ?? 0,
                MainMeasureUnitId = item.MainMeasureUnitId ?? 0,
                SecondaryMeasureUnitId = item.SecondaryMeasureUnitId ?? 0,
                BuyUnitToMainRate = item.BuyUnitToMainRate,
                SecondaryUnitToMainRate = item.SecondaryUnitToMainRate,
                FpaDefId = item.FpaDefId,
                MaterialCategoryId = item.MaterialCategoryId,
                MaterialType = item.MaterialType,
                WarehouseItemNature = item.WarehouseItemNature,
                ManufacturerCode = item.ManufacturerCode,
                BarCode = item.BarCode,
                PriceNetto = item.PriceNetto,
                PriceBrutto = item.PriceBrutto,
                CompanyId = item.CompanyId,
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now
            };

            await _context.WarehouseItems.AddAsync(warehouseItem);
            await _context.SaveChangesAsync();
            newEntity = _context.Entry(warehouseItem).Entity;
            if (!string.IsNullOrEmpty(item.SelectedCompanies))
            {
                var companiesSelected = JsonSerializer.Deserialize<string[]>(item.SelectedCompanies);
                if (companiesSelected != null)
                {
                    foreach (var i in companiesSelected)
                    {
                        if (Int32.TryParse(i, out int compId))
                        {
                            warehouseItem.CompanyMappings.Add(new CompanyWarehouseItemMapping
                            {
                                CompanyId = compId,
                                WarehouseItemId = newEntity.Id
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            if (ownsTransaction) await transaction.CommitAsync();

            return ServiceResult.Ok(newEntity);
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while adding warehouse item");
            if (ex.GetBaseException() is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return ServiceResult.Error("Το είδος με αυτόν τον κωδικό υπάρχει ήδη", "DUPLICATE_VALUE");
                }
            }
            return ServiceResult.Error($"Error: {ex.Message}", "INTERNAL_ERROR");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<ServiceResult> ModifyWarehouseItemAsync(WarehouseItemModifyDto item)
    {
        _logger.LogInformation("Modifying warehouse item: {Item}", item);
        if (item is null || item.Id <= 0)
        {
            return ServiceResult.Error("Δεν υπάρχει είδος προς ενημέρωση", "BADREQUEST");
        }

        var dbItem = await _context.WarehouseItems
            .Include(x => x.CompanyMappings)
            .FirstOrDefaultAsync(x => x.Id == item.Id);

        if (dbItem == null)
        {
            return ServiceResult.Error("Το είδος δεν βρέθηκε", "NOTFOUND");
        }

        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;

        try
        {
            dbItem.Active = item.Active;
            dbItem.Name = item.Name;
            dbItem.ShortDescription = item.ShortDescription;
            dbItem.Description = item.Description;
            dbItem.Code = item.Code;
            dbItem.BuyMeasureUnitId = item.BuyMeasureUnitId ?? 0;
            dbItem.MainMeasureUnitId = item.MainMeasureUnitId ?? 0;
            dbItem.SecondaryMeasureUnitId = item.SecondaryMeasureUnitId ?? 0;
            dbItem.BuyUnitToMainRate = item.BuyUnitToMainRate;
            dbItem.SecondaryUnitToMainRate = item.SecondaryUnitToMainRate;
            dbItem.FpaDefId = item.FpaDefId;
            dbItem.MaterialCategoryId = item.MaterialCategoryId;
            dbItem.MaterialType = item.MaterialType;
            dbItem.WarehouseItemNature = item.WarehouseItemNature;
            dbItem.ManufacturerCode = item.ManufacturerCode;
            dbItem.BarCode = item.BarCode;
            dbItem.PriceNetto = item.PriceNetto;
            dbItem.PriceBrutto = item.PriceBrutto;
            dbItem.CompanyId = item.CompanyId;
            dbItem.DateLastModified = DateTime.Now;

            // Update Company Mappings
            if (!string.IsNullOrEmpty(item.SelectedCompanies))
            {
                var selectedCompanyIds = JsonSerializer.Deserialize<string[]>(item.SelectedCompanies)?
                    .Select(s => int.TryParse(s, out int id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList() ?? new List<int>();

                // Remove mappings not in the selected list
                var mappingsToRemove = dbItem.CompanyMappings
                    .Where(m => !selectedCompanyIds.Contains(m.CompanyId))
                    .ToList();
                foreach (var mapping in mappingsToRemove)
                {
                    dbItem.CompanyMappings.Remove(mapping);
                }

                // Add new mappings
                var existingCompanyIds = dbItem.CompanyMappings.Select(m => m.CompanyId).ToList();
                foreach (var compId in selectedCompanyIds)
                {
                    if (!existingCompanyIds.Contains(compId))
                    {
                        dbItem.CompanyMappings.Add(new CompanyWarehouseItemMapping
                        {
                            CompanyId = compId,
                            WarehouseItemId = dbItem.Id
                        });
                    }
                }
            }
            else
            {
                dbItem.CompanyMappings.Clear();
            }

            _context.WarehouseItems.Update(dbItem);
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction.CommitAsync();

            return ServiceResult.Ok(dbItem);
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while modifying warehouse item");
            if (ex.GetBaseException() is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return ServiceResult.Error("Το είδος με αυτόν τον κωδικό υπάρχει ήδη", "DUPLICATE_VALUE");
                }
            }
            return ServiceResult.Error($"Error: {ex.Message}", "INTERNAL_ERROR");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<ServiceResult> DeleteWarehouseItemAsync(int itemId)
    {
        _logger.LogInformation("Deleting warehouse item: {ItemId}", itemId);
        if (itemId <= 0)
        {
            return ServiceResult.Error("Δεν υπάρχει είδος προς διαγραφή", "BADREQUEST");
        }
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {


            var item = await _context.WarehouseItems.FindAsync(itemId);
            if (item == null)
            {
                return ServiceResult.Error("Το είδος δεν βρέθηκε", "NOTFOUND");
            }

            _context.CompanyWarehouseItemMappings.RemoveRange(
                _context.CompanyWarehouseItemMappings.Where(p => p.WarehouseItemId == itemId));
            _context.WarehouseItems.Remove(item);
            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction.CommitAsync();

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while deleting warehouse item");
            return ServiceResult.Error($"Error: {ex.Message}", "INTERNAL_ERROR");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<ServiceResult> AddCompanyMappingToWarehouseItemAsync(int warehouseItemId, int companyId)
    {
        _logger.LogInformation("AddCompanyMappingToWarehouseItemAsync: {warehouseItemId}, {companyId}", warehouseItemId, companyId);
        if (warehouseItemId <= 0 || companyId <= 0)
        {
            return ServiceResult.Error("Invalid warehouse item or company", "BADREQUEST");
        }
        bool ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : _context.Database.CurrentTransaction;
        try
        {
            var mapping = new CompanyWarehouseItemMapping
            {
                CompanyId = companyId,
                WarehouseItemId = warehouseItemId
            };
            await _context.CompanyWarehouseItemMappings.AddAsync(mapping);
               
            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction.CommitAsync();

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while deleting warehouse item");
            return ServiceResult.Error($"Error: {ex.Message}", "INTERNAL_ERROR");
        }
        finally
        {
            if (ownsTransaction && transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}