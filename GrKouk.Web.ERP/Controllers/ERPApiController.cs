using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Domain.Sync;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.Sync;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace GrKouk.Web.ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpApiController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<ErpApiController> _logger;
        private readonly IDocumentTransactionService _docTransSrv;
        private readonly IDocumentSyncService _docSyncSrv;
        private readonly IWarehouseManagementSrv _warehouseManagementSrv;

        public ErpApiController(ApiDbContext context, ILogger<ErpApiController> logger,
            IDocumentTransactionService docTransSrv, IDocumentSyncService docSyncSrv,IWarehouseManagementSrv warehouseManagementSrv)
        {
            _context = context;
            _logger = logger;
            _docTransSrv = docTransSrv;
            _docSyncSrv = docSyncSrv;
            _warehouseManagementSrv = warehouseManagementSrv;
        }

        [HttpPost("SyncBusinessItemFamilies")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncBusinessItemFamilies([FromBody] SyncBusinessItemFamilyRequest request)
        {
            // _logger.LogInformation("SyncBusinessItemFamilies");
            // int addedCount = 0;
            // int failedToAddCount = 0;
            // int updatedCount = 0;
            // int failedToUpdateCount = 0;
            // int deletedCount = 0;
            // int failedToDeleteCount = 0;
            //
            // if (request == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "Empty request data"
            //     });
            // }
            //
            // if (request.Items == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Items"
            //     });
            // }
            //
            // if (request.CompanyCode == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No CompanyCode"
            //     });
            // }
            //
            // string businessCompanyCode = request.CompanyCode;
            // var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == businessCompanyCode);
            // if (company == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Company for this company code"
            //     });
            // }
            //
            // int companyId = company.Id;
            //
            // var sourceList = new List<SyncItemFamily>();
            //
            //
            // foreach (var item in request.Items)
            // {
            //     var sourceItem = new SyncItemFamily
            //     {
            //         Id = Guid.NewGuid(),
            //         BusId = item.Id,
            //         Name = item.Name,
            //         SourceChecksum = ChecksumHelper.CalculateChecksum(item.Id.ToString(), item.Name)
            //     };
            //     sourceList.Add(sourceItem);
            // }
            //
            // var sourceDict = sourceList.ToDictionary(x => x.BusId);
            // var destinationItems = _context.SyncItemFamilies.ToList();
            // var destinationDict = destinationItems.ToDictionary(x => x.BusId);
            //
            // var toInsert = sourceDict
            //     .Where(src => !destinationDict.ContainsKey(src.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toUpdate = sourceDict
            //     .Where(src =>
            //         destinationDict.ContainsKey(src.Key) &&
            //         destinationDict[src.Key].SourceChecksum != src.Value.SourceChecksum)
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toDelete = destinationDict
            //     .Where(dest => !sourceDict.ContainsKey(dest.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var syncSessionId = Guid.NewGuid(); // Unique session ID for this sync operation
            // var syncSource = "MAUI Client"; // Source of the sync operation
            //
            // await using var transaction = await _context.Database.BeginTransactionAsync();
            // try
            // {
            //     foreach (var item in toInsert)
            //     {
            //         //Insert to the main MaterialCategories 
            //         var newMaterialCat = _context.MaterialCategories.Add(new MaterialCategory
            //         {
            //             Code = item.BusId.ToString(),
            //             Name = item.Name,
            //             CompanyId = companyId
            //         });
            //         try
            //         {
            //             await _context.SaveChangesAsync();
            //         }
            //         catch (Exception ex)
            //         {
            //             await transaction.RollbackAsync();
            //             failedToAddCount++;
            //             _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //             return BadRequest(new
            //             {
            //                 error = "SyncBusinessItemFamilies error " + ex.Message
            //             });
            //         }
            //
            //         item.ErpId = newMaterialCat.Entity.Id;
            //         var newSyncItem = _context.SyncItemFamilies.Add(item);
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = "SyncItemFamily",
            //             EntityId = item.BusId,
            //             OperationType = "INSERT",
            //             Source = syncSource,
            //         });
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = "MaterialCategory",
            //             EntityId = newMaterialCat.Entity.Id,
            //             OperationType = "INSERT",
            //             Source = syncSource,
            //         });
            //         addedCount++;
            //         //await _context.SaveChangesAsync();
            //     }
            //
            //     foreach (var item in toUpdate)
            //     {
            //         _context.SyncItemFamilies.Update(item);
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = "SyncItemFamily",
            //             EntityId = item.BusId,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         var materialId = item.ErpId;
            //         var materialToUpdate =
            //             await _context.MaterialCategories.FirstOrDefaultAsync(p => p.Id == materialId);
            //         if (materialToUpdate == null)
            //         {
            //             _logger.LogError("Material not found for id {Id}", materialId);
            //             continue;
            //         }
            //
            //         materialToUpdate.Name = item.Name;
            //         materialToUpdate.Code = item.BusId.ToString();
            //         _context.MaterialCategories.Update(materialToUpdate);
            //
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = "MaterialCategory",
            //             EntityId = materialToUpdate.Id,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         updatedCount++;
            //     }
            //
            //     foreach (var item in toDelete)
            //     {
            //         deletedCount++;
            //         _context.SyncItemFamilies.Remove(item);
            //     }
            //
            //     await _context.SaveChangesAsync();
            //     await transaction.CommitAsync();
            // }
            // catch (Exception ex)
            // {
            //     await transaction.RollbackAsync();
            //     _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //     return BadRequest(new
            //     {
            //         error = "SyncBusinessItemFamilies error " + ex.Message
            //     });
            // }
            //
            // var res = new ErpSynchronizationResponse<SyncItemFamily>
            // {
            //     Message = "SyncBusinessItemFamilies",
            //     AddedCount = addedCount,
            //     FailedToAddCount = failedToAddCount,
            //     UpdatedCount = updatedCount,
            //     FailedToUpdateCount = failedToUpdateCount,
            //     DeletedCount = deletedCount,
            //     FailedToDeleteCount = failedToDeleteCount,
            //     SyncSessionId = syncSessionId,
            //     SyncSource = syncSource,
            //     SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            // };
            //return Ok(res); 
            return Ok();
        }

        [HttpPost("SyncBusinessUnitsOfMeasurement")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncBusinessUnitsOfMeasurement(
            [FromBody] SyncBusinessUnitsOfMeasurementRequest request)
        {
            // string mainEntityName = "SyncUnitOfMeasurement";
            // string syncEntityName = "SyncUnitOfMeasurement";
            // _logger.LogInformation("SyncBusinessUnitsOfMeasurement");
            // int addedCount = 0;
            // int failedToAddCount = 0;
            // int updatedCount = 0;
            // int failedToUpdateCount = 0;
            // int deletedCount = 0;
            // int failedToDeleteCount = 0;
            // if (request == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "Empty request data"
            //     });
            // }
            //
            // if (request.Items == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Items"
            //     });
            // }
            //
            // if (request.CompanyCode == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No CompanyCode"
            //     });
            // }
            //
            // string businessCompanyCode = request.CompanyCode;
            // var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == businessCompanyCode);
            // if (company == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Company for this company code"
            //     });
            // }
            //
            // int companyId = company.Id;
            //
            // var sourceList = new List<SyncUnitOfMeasurement>();
            //
            //
            // foreach (var item in request.Items)
            // {
            //     var sourceItem = new SyncUnitOfMeasurement
            //     {
            //         BusId = item.Id,
            //         Name = item.Name,
            //         SourceChecksum = ChecksumHelper.CalculateChecksum(item.Id.ToString(), item.Name)
            //     };
            //     sourceList.Add(sourceItem);
            // }
            //
            // #region Dictionaries
            //
            // var sourceDict = sourceList.ToDictionary(x => x.BusId);
            // var destinationItems = _context.SyncUnitOfMeasurements.ToList();
            // var destinationDict = destinationItems.ToDictionary(x => x.BusId);
            //
            // var toInsert = sourceDict
            //     .Where(src => !destinationDict.ContainsKey(src.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toUpdate = sourceDict
            //     .Where(src =>
            //         destinationDict.ContainsKey(src.Key) &&
            //         destinationDict[src.Key].SourceChecksum != src.Value.SourceChecksum)
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toDelete = destinationDict
            //     .Where(dest => !sourceDict.ContainsKey(dest.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // #endregion
            //
            // var syncSessionId = Guid.NewGuid(); // Unique session ID for this sync operation
            // var syncSource = "MAUI Client"; // Source of the sync operation
            //
            // await using var transaction = await _context.Database.BeginTransactionAsync();
            // try
            // {
            //     foreach (var item in toInsert)
            //     {
            //         //Insert to the main entities 
            //         var newMainItem = _context.MeasureUnits.Add(new MeasureUnit()
            //         {
            //             Code = item.BusId.ToString(),
            //             Name = item.Name,
            //             //CompanyId = companyId
            //         });
            //         try
            //         {
            //             await _context.SaveChangesAsync();
            //         }
            //         catch (Exception ex)
            //         {
            //             await transaction.RollbackAsync();
            //             _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //             failedToAddCount++;
            //             return BadRequest(new
            //             {
            //                 error = "SyncBusinessUnitsOfMeasurement error " + ex.Message
            //             });
            //         }
            //
            //         item.ErpId = newMainItem.Entity.Id;
            //         var newSyncItem = _context.SyncUnitOfMeasurements.Add(item);
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = syncEntityName,
            //             EntityId = item.BusId,
            //             OperationType = "INSERT",
            //             Source = syncSource,
            //         });
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = mainEntityName,
            //             EntityId = newMainItem.Entity.Id,
            //             OperationType = "INSERT",
            //             Source = syncSource,
            //         });
            //         addedCount++;
            //         //await _context.SaveChangesAsync();
            //     }
            //
            //     foreach (var item in toUpdate)
            //     {
            //         _context.SyncUnitOfMeasurements.Update(item);
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = syncEntityName,
            //             EntityId = item.BusId,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         var mainEntityId = item.ErpId;
            //         var mainEntityToUpdate =
            //             await _context.MeasureUnits.FirstOrDefaultAsync(p => p.Id == mainEntityId);
            //         if (mainEntityToUpdate == null)
            //         {
            //             _logger.LogError("Main entity with id {Id} not found", mainEntityId);
            //             continue;
            //         }
            //
            //         mainEntityToUpdate.Name = item.Name;
            //         mainEntityToUpdate.Code = item.BusId.ToString();
            //         _context.MeasureUnits.Update(mainEntityToUpdate);
            //
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = mainEntityName,
            //             EntityId = mainEntityToUpdate.Id,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         updatedCount++;
            //     }
            //
            //     foreach (var item in toDelete)
            //     {
            //         _context.SyncUnitOfMeasurements.Remove(item);
            //         deletedCount++;
            //     }
            //
            //     await _context.SaveChangesAsync();
            //     await transaction.CommitAsync();
            // }
            // catch (Exception ex)
            // {
            //     await transaction.RollbackAsync();
            //     _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //     return BadRequest(new
            //     {
            //         error = "SyncBusinessUnitsOfMeasurement error " + ex.Message
            //     });
            // }
            //
            //
            // var res = new ErpSynchronizationResponse<SyncUnitOfMeasurement>
            // {
            //     Message = "SyncBusinessItemFamilies",
            //     AddedCount = addedCount,
            //     FailedToAddCount = failedToAddCount,
            //     UpdatedCount = updatedCount,
            //     FailedToUpdateCount = failedToUpdateCount,
            //     DeletedCount = deletedCount,
            //     FailedToDeleteCount = failedToDeleteCount,
            //     SyncSessionId = syncSessionId,
            //     SyncSource = syncSource,
            //     SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            // };
            // return Ok(res);
            return Ok();
        }

        [HttpPost("SyncBuyDocuments")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncBuyDocuments([FromBody] SyncBusinessBuyDocumentsRequest request)
        {
            #region Boiler Plate Code

            string mainEntityName = SyncEntityNames.SyncBuyDocument;
            //string syncEntityName = SyncEntityNames.SyncBuyDocument;
            
            _logger.LogInformation("SyncBuyDocuments");
            int addedCount = 0;
            int failedToAddCount = 0;
            int updatedCount = 0;
            int failedToUpdateCount = 0;
            int deletedCount = 0;
            int failedToDeleteCount = 0;
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            if (request.Items == null)
            {
                return BadRequest(new
                {
                    error = "No Items"
                });
            }

            if (request.CompanyCode == null)
            {
                return BadRequest(new
                {
                    error = "No Company Code"
                });
            }

            string businessCompanyCode = request.CompanyCode;
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == businessCompanyCode);
            if (company == null)
            {
                return BadRequest(new
                {
                    error = "No Company for this company code"
                });
            }

            #endregion

            int companyId = company.Id;

            var sourceList = new List<SyncBuyDocument>();


            foreach (var item in request.Items)
            {
                var sourceItem = new SyncBuyDocument
                {
                    BusId = item.Id,
                    TransDate = item.TransDate,
                    SupplierId = item.SupplierId,
                    RefNumber = item.RefNumber,
                    TotalAmount = item.TotalAmount,
                    PayedAmount = item.PayedAmount,
                    SourceChecksum = ChecksumHelper.CalculateChecksum(item.Id.ToString()
                        , item.TransDate.ToString(CultureInfo.InvariantCulture), item.SupplierId.ToString()
                        , item.RefNumber.ToString(), item.TotalAmount.ToString(CultureInfo.InvariantCulture)
                        , item.PayedAmount.ToString(CultureInfo.InvariantCulture))
                };
                sourceList.Add(sourceItem);
            }

            #region Dictionaries

            var sourceDict = sourceList.ToDictionary(x => x.BusId);
            var destinationItems = _context.SyncBuyDocuments.ToList();
            var destinationDict = destinationItems.ToDictionary(x => x.BusId);

            var toInsert = sourceDict
                .Where(src => !destinationDict.ContainsKey(src.Key))
                .Select(pair => pair.Value)
                .ToList();

            var toUpdate = sourceDict
                .Where(src =>
                    destinationDict.ContainsKey(src.Key) &&
                    destinationDict[src.Key].SourceChecksum != src.Value.SourceChecksum)
                .Select(pair => pair.Value)
                .ToList();

            var toDelete = destinationDict
                .Where(dest => !sourceDict.ContainsKey(dest.Key))
                .Select(pair => pair.Value)
                .ToList();

            #endregion

            var syncSessionId = Guid.NewGuid(); // Unique session ID for this sync operation
            var syncSource = "MAUI Client"; // Source of the sync operation

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in toInsert)
                {
                    //Create a new buy document create Dto
                    //Must find erp supplier transactor id based on business supplier id
                    var newMainItem = _context.MeasureUnits.Add(new MeasureUnit()
                    {
                        Code = item.BusId.ToString(),
                        // Name = item.Name,
                        //CompanyId = companyId
                    });
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
                        failedToAddCount++;
                        return BadRequest(new
                        {
                            error = "SyncBuyDocuments error " + ex.Message
                        });
                    }

                    _context.SynchronizationLogs.Add(new SynchronizationLog
                    {
                        SyncSessionId = syncSessionId,
                        EntityName = mainEntityName,
                        // EntityId = newMainItem.Entity.Id,
                        OperationType = "INSERT",
                        Source = syncSource,
                    });
                    addedCount++;
                    //await _context.SaveChangesAsync();
                }

                // foreach (var item in toUpdate)
                // {
                //     // _context.SyncUnitOfMeasurements.Update(item);
                //     _context.SynchronizationLogs.Add(new SynchronizationLog
                //     {
                //         SyncSessionId = syncSessionId,
                //         EntityName = syncEntityName,
                //        // EntityId = item.BusId,
                //         OperationType = "UPDATE",
                //         Source = syncSource,
                //     });
                //     var mainEntityId = item.ErpId;
                //     var mainEntityToUpdate =
                //         await _context.MeasureUnits.FirstOrDefaultAsync(p => p.Id == mainEntityId);
                //     if (mainEntityToUpdate == null)
                //     {
                //         _logger.LogError("Main entity with id {Id} not found", mainEntityId);
                //         continue;
                //     }
                //
                //     // mainEntityToUpdate.Name = item.Name;
                //     mainEntityToUpdate.Code = item.BusId.ToString();
                //     _context.MeasureUnits.Update(mainEntityToUpdate);
                //
                //
                //     _context.SynchronizationLogs.Add(new SynchronizationLog
                //     {
                //         SyncSessionId = syncSessionId,
                //         EntityName = mainEntityName,
                //         EntityId = mainEntityToUpdate.Id,
                //         OperationType = "UPDATE",
                //         Source = syncSource,
                //     });
                //     updatedCount++;
                // }

                // foreach (var item in toDelete)
                // {
                //     // _context.SyncUnitOfMeasurements.Remove(item);
                //     deletedCount++;
                // }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
                return BadRequest(new
                {
                    error = "SyncBuyDocuments error " + ex.Message
                });
            }


            var res = new ErpSynchronizationResponse<SyncBuyDocument>
            {
                Message = "SyncBuyDocuments",
                AddedCount = addedCount,
                FailedToAddCount = failedToAddCount,
                UpdatedCount = updatedCount,
                FailedToUpdateCount = failedToUpdateCount,
                DeletedCount = deletedCount,
                FailedToDeleteCount = failedToDeleteCount,
                SyncSessionId = syncSessionId,
                SyncSource = syncSource,
                SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            };
            return Ok(res);
        }

        [HttpPost("SyncMatchedBusinessSuppliers")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncMatchedBusinessSuppliers(
            [FromBody] SyncBusinessEntityRequest<SyncSupplierDto> request)
        {
            #region BoilerPlate Code

            string mainEntityName = "SyncSupplier";
            //string syncEntityName = "SyncSupplier";
            _logger.LogInformation("SyncMatchedBusinessSuppliers");
            int addedCount = 0;
            int failedToAddCount = 0;
            int updatedCount = 0;
            int failedToUpdateCount = 0;
            int deletedCount = 0;
            int failedToDeleteCount = 0;
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            if (request.Items == null)
            {
                return BadRequest(new
                {
                    error = "No Items"
                });
            }

            if (request.CompanyCode == null)
            {
                return BadRequest(new
                {
                    error = "No Company Code"
                });
            }

            string businessCompanyCode = request.CompanyCode;
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == businessCompanyCode);
            if (company == null)
            {
                return BadRequest(new
                {
                    error = "No Company for this company code"
                });
            }

            int companyId = company.Id;

            #endregion

            var syncSessionId = Guid.NewGuid(); // Unique session ID for this sync operation
            var syncSource = "MAUI Client"; // Source of the sync operation
            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var item in request.Items)
            {
                var newItem = new SyncSupplier
                {
                    Id = Guid.NewGuid(),
                    BusId = item.BusId,
                    ErpId = item.ErpId,
                    Name = item.Name,
                    TaxNumber = item.TaxNumber,
                    BusCode = item.BusCode,
                    CompanyCode = item.CompanyCode,
                    SourceChecksum = item.SourceChecksum
                };
                var newMainItem = _context.SyncSuppliers.Add(newItem);

                _context.SynchronizationLogs.Add(new SynchronizationLog
                {
                    Id = Guid.NewGuid(),
                    SyncSessionId = syncSessionId,
                    EntityName = mainEntityName,
                    EntityId = newMainItem.Entity.Id,
                    CompanyCode = newMainItem.Entity.CompanyCode,
                    OperationType = "INSERT",
                    Source = syncSource,
                });
                addedCount++;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
                    failedToAddCount++;
                    return BadRequest(new
                    {
                        error = "SyncSupplier error " + ex.Message
                    });
                }
            }

            await transaction.CommitAsync();
            var res = new ErpSynchronizationResponse<SyncBuyDocument>
            {
                Message = "Sync Suppliers",
                AddedCount = addedCount,
                FailedToAddCount = failedToAddCount,
                UpdatedCount = updatedCount,
                FailedToUpdateCount = failedToUpdateCount,
                DeletedCount = deletedCount,
                FailedToDeleteCount = failedToDeleteCount,
                SyncSessionId = syncSessionId,
                SyncSource = syncSource,
                //SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            };
            return Ok(res);
        }

        [HttpPost("SyncBusinessSuppliers")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncBusinessSuppliers(
            [FromBody] SyncBusinessEntityRequest<SyncBusinessSupplierDto> request)
        {
            // #region BoilerPlate Code
            //
            // string mainEntityName = "SyncSupplier";
            // string syncEntityName = "SyncSupplier";
            // _logger.LogInformation("SyncBusinessSuppliers");
            // int addedCount = 0;
            // int failedToAddCount = 0;
            // int updatedCount = 0;
            // int failedToUpdateCount = 0;
            // int deletedCount = 0;
            // int failedToDeleteCount = 0;
            // if (request == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "Empty request data"
            //     });
            // }
            //
            // if (request.Items == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Items"
            //     });
            // }
            //
            // if (request.CompanyCode == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Company Code"
            //     });
            // }
            //
            // string businessCompanyCode = request.CompanyCode;
            // var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == businessCompanyCode);
            // if (company == null)
            // {
            //     return BadRequest(new
            //     {
            //         error = "No Company for this company code"
            //     });
            // }
            //
            // int companyId = company.Id;
            //
            // #endregion
            //
            // var sourceList = new List<SyncSupplier>();
            //
            //
            // foreach (var item in request.Items)
            // {
            //     var sourceItem = new SyncSupplier
            //     {
            //         BusId = item.Id,
            //         Name = item.Name,
            //         TaxNumber = item.TaxNumber,
            //         BusCode = item.Code,
            //         SourceChecksum = ChecksumHelper.CalculateChecksum(item.Id.ToString()
            //             , item.Name
            //             , item.Code
            //             , item.TaxNumber)
            //     };
            //     sourceList.Add(sourceItem);
            // }
            //
            // #region Dictionaries
            //
            // var sourceDict = sourceList.ToDictionary(x => x.BusId);
            // var destinationItems = _context.SyncSuppliers.ToList();
            // var destinationDict = destinationItems.ToDictionary(x => x.BusId);
            //
            // var toInsert = sourceDict
            //     .Where(src => !destinationDict.ContainsKey(src.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toUpdate = sourceDict
            //     .Where(src =>
            //         destinationDict.ContainsKey(src.Key) &&
            //         destinationDict[src.Key].SourceChecksum != src.Value.SourceChecksum)
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // var toDelete = destinationDict
            //     .Where(dest => !sourceDict.ContainsKey(dest.Key))
            //     .Select(pair => pair.Value)
            //     .ToList();
            //
            // #endregion
            //
            // var syncSessionId = Guid.NewGuid(); // Unique session ID for this sync operation
            // var syncSource = "MAUI Client"; // Source of the sync operation
            //
            // await using var transaction = await _context.Database.BeginTransactionAsync();
            // try
            // {
            //     foreach (var item in toInsert)
            //     {
            //        //Create a new buy document create Dto
            //        //Must find erp supplier transactor id based on business supplier id
            //         var newMainItem = _context.MeasureUnits.Add(new MeasureUnit()
            //         {
            //             Code = item.BusId.ToString(),
            //             // Name = item.Name,
            //             //CompanyId = companyId
            //         });
            //         try
            //         {
            //             await _context.SaveChangesAsync();
            //         }
            //         catch (Exception ex)
            //         {
            //             await transaction.RollbackAsync();
            //             _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //             failedToAddCount++;
            //             return BadRequest(new
            //             {
            //                 error = "SyncBuyDocuments error " + ex.Message
            //             });
            //         }
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = mainEntityName,
            //             EntityId = newMainItem.Entity.Id,
            //             OperationType = "INSERT",
            //             Source = syncSource,
            //         });
            //         addedCount++;
            //         //await _context.SaveChangesAsync();
            //     }
            //
            //     foreach (var item in toUpdate)
            //     {
            //         // _context.SyncUnitOfMeasurements.Update(item);
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = syncEntityName,
            //             EntityId = item.BusId,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         var mainEntityId = item.ErpId;
            //         var mainEntityToUpdate =
            //             await _context.MeasureUnits.FirstOrDefaultAsync(p => p.Id == mainEntityId);
            //         if (mainEntityToUpdate == null)
            //         {
            //             _logger.LogError("Main entity with id {Id} not found", mainEntityId);
            //             continue;
            //         }
            //
            //         // mainEntityToUpdate.Name = item.Name;
            //         mainEntityToUpdate.Code = item.BusId.ToString();
            //         _context.MeasureUnits.Update(mainEntityToUpdate);
            //
            //
            //         _context.SynchronizationLogs.Add(new SynchronizationLog
            //         {
            //             SyncSessionId = syncSessionId,
            //             EntityName = mainEntityName,
            //             EntityId = mainEntityToUpdate.Id,
            //             OperationType = "UPDATE",
            //             Source = syncSource,
            //         });
            //         updatedCount++;
            //     }
            //
            //     foreach (var item in toDelete)
            //     {
            //         // _context.SyncUnitOfMeasurements.Remove(item);
            //         deletedCount++;
            //     }
            //
            //     await _context.SaveChangesAsync();
            //     await transaction.CommitAsync();
            // }
            // catch (Exception ex)
            // {
            //     await transaction.RollbackAsync();
            //     _logger.LogError("An error occurred during synchronization: {Error}", ex.Message);
            //     return BadRequest(new
            //     {
            //         error = "SyncBuyDocuments error " + ex.Message
            //     });
            // }
            //
            //
            // var res = new ErpSynchronizationResponse<SyncBuyDocument>
            // {
            //     Message = "SyncBuyDocuments",
            //     AddedCount = addedCount,
            //     FailedToAddCount = failedToAddCount,
            //     UpdatedCount = updatedCount,
            //     FailedToUpdateCount = failedToUpdateCount,
            //     DeletedCount = deletedCount,
            //     FailedToDeleteCount = failedToDeleteCount,
            //     SyncSessionId = syncSessionId,
            //     SyncSource = syncSource,
            //     //SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            // };
            // return Ok(res);
            return Ok();
        }


        [HttpGet("GetErpSyncSuppliers")]
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

        [HttpGet("GetErpSuppliers")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> GetErpSuppliers(string companyCode)
        {
            const string supplierTypeCode = "SYS.SUPPLIER";
            int allCompaniesId = 0;
            var allCompCode =
                await _context.AppSettings.SingleOrDefaultAsync(p => p.Code == Constants.AllCompaniesCodeKey);
            if (allCompCode == null)
            {
                return NotFound("All Companies Code Setting not found");
            }

            var allCompaniesEntity =
                await _context.Companies.SingleOrDefaultAsync(s => s.Code == allCompCode.Value);

            if (allCompaniesEntity != null)
            {
                allCompaniesId = allCompaniesEntity.Id;
            }

            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
            if (company == null)
            {
                return BadRequest("No Company for this company code");
            }

            var companyId = company.Id;

            var fullListIq = _context.TransactorCompanyMappings
                .Select(t => new TransactorBigClass
                {
                    Id = t.Transactor.Id,
                    Code = t.Transactor.Code,
                    Name = t.Transactor.Name,
                    TaxNumber = t.Transactor.TaxNumber,
                    TransactorTypeId = t.Transactor.TransactorTypeId,
                    TransactorTypeCode = t.Transactor.TransactorType.Code,
                    TransactorTypeName = t.Transactor.TransactorType.Name,
                    CompanyId = t.Company.Id,
                    CompanyCode = t.Company.Code
                });

            fullListIq = fullListIq.Where(t =>
                (t.CompanyId == companyId || t.CompanyId == allCompaniesId) &&
                t.TransactorTypeCode == supplierTypeCode);

            var testList = fullListIq.ToList();
            var projectedList = testList.GroupBy(g => new
                {
                    g.Id,
                    g.Name,
                    g.Code,
                    g.TaxNumber
                })
                .Select(f => new ErpSupplierDto()
                {
                    Id = f.Key.Id,
                    Name = f.Key.Name,
                    Code = f.Key.Code,
                    Afm = f.Key.TaxNumber
                });
            var listItems = projectedList.OrderBy(p => p.Name).ToList();

            return Ok(listItems);
        }

        [HttpGet("GetErpMeasureUnits")]
        [Authorize(Policy = "ApiPolicy2")]
        // [AllowAnonymous]
        public async Task<IActionResult> GetErpMeasureUnits(string companyCode)
        {
            
             int allCompaniesId = 0;
             var allCompCode =
                 await _context.AppSettings.SingleOrDefaultAsync(p => p.Code == Constants.AllCompaniesCodeKey);
             if (allCompCode == null)
             {
                 return NotFound("All Companies Code Setting not found");
             }
            
             var allCompaniesEntity =
                 await _context.Companies.SingleOrDefaultAsync(s => s.Code == allCompCode.Value);
            
             if (allCompaniesEntity != null)
             {
                 allCompaniesId = allCompaniesEntity.Id;
             }
             var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
             if (company == null)
             {
                 return BadRequest("No Company for this company code");
             }
             var companyId = company.Id;
            var items = await _context.MeasureUnits.Where(p =>
                p.Active
            ).ToListAsync();
            return Ok(items);
        }
        [HttpGet("GetErpVatClasses")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> GetErpVatClasses(string companyCode)
        {
            int allCompaniesId = 0;
            var allCompCode =
                await _context.AppSettings.SingleOrDefaultAsync(p => p.Code == Constants.AllCompaniesCodeKey);
            if (allCompCode == null)
            {
                return NotFound("All Companies Code Setting not found");
            }
            
            var allCompaniesEntity =
                await _context.Companies.SingleOrDefaultAsync(s => s.Code == allCompCode.Value);
            
            if (allCompaniesEntity != null)
            {
                allCompaniesId = allCompaniesEntity.Id;
            }
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
            if (company == null)
            {
                return BadRequest("No Company for this company code");
            }
            var companyId = company.Id;
            var items = await _context.FpaKategories.ToListAsync();
            return Ok(items);
        }
        /// <summary>
        /// To be synced with Item Family in Business App
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        [HttpGet("GetErpItemCategories")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> GetErpWarehouseItemCategories(string companyCode)
        {
            var query = _context.MaterialCategories.AsQueryable();

            // Return all categories if companyCode is null, empty, or "ALL"
            if (string.IsNullOrEmpty(companyCode) || companyCode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var allItems = await query.ToListAsync();
                return Ok(allItems);
            }

            int allCompaniesId = 0;
            var allCompCode =
                await _context.AppSettings.SingleOrDefaultAsync(p => p.Code == Constants.AllCompaniesCodeKey);
            if (allCompCode == null)
            {
                return NotFound(new { error = "All Companies Code Setting not found" });
            }
            
            var allCompaniesEntity =
                await _context.Companies.SingleOrDefaultAsync(s => s.Code == allCompCode.Value);
            
            if (allCompaniesEntity != null)
            {
                allCompaniesId = allCompaniesEntity.Id;
            }
            // Filter by specific company
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
            if (company == null)
            {
                return NotFound(new { ErrorMessage = "No Company found for this company code" });
            }

            query = query.Where(p => p.CompanyId == company.Id || p.CompanyId == allCompaniesId);
            var items = await query.ToListAsync();

            return Ok(items);
        }
        
        [HttpGet("GetErpPaymentMethods")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> GetErpPaymentMethods(string companyCode)
        {
            var query = _context.PaymentMethods.AsQueryable();

            // Return all categories if companyCode is null, empty, or "ALL"
            if (string.IsNullOrEmpty(companyCode) || companyCode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var allItems = await query.ToListAsync();
                return Ok(allItems);
            }

            int allCompaniesId = 0;
            var allCompCode =
                await _context.AppSettings.SingleOrDefaultAsync(p => p.Code == Constants.AllCompaniesCodeKey);
            if (allCompCode == null)
            {
                return NotFound(new { error = "All Companies Code Setting not found" });
            }
            
            var allCompaniesEntity =
                await _context.Companies.SingleOrDefaultAsync(s => s.Code == allCompCode.Value);
            
            if (allCompaniesEntity != null)
            {
                allCompaniesId = allCompaniesEntity.Id;
            }
            // Filter by specific company
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
            if (company == null)
            {
                return NotFound(new { ErrorMessage = "No Company found for this company code" });
            }

            query = query.Where(p => p.CompanyId == company.Id || p.CompanyId == allCompaniesId);
            var items = await query.ToListAsync();

            return Ok(items);
        }
        [HttpGet("GetErpItemsForCashier")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> GetErpItemsForCashier(string companyCode)
        {
            var query = _context.CompanyWarehouseItemMappings
                .Include(p => p.WarehouseItem)
                
                .AsQueryable();

            // Return all categories if companyCode is null, empty, or "ALL"
            if (string.IsNullOrEmpty(companyCode) || companyCode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var allItems = await query.ToListAsync();
                return Ok(allItems);
            }
            
            // Filter by specific company
            var company = await _context.Companies.SingleOrDefaultAsync(p => p.Code == companyCode);
            if (company == null)
            {
                return NotFound(new { ErrorMessage = "No Company found for this company code" });
            }

            query = query.Where(p => p.CompanyId == company.Id );
            var items = await query
                .Select(i=> new ErpItemDto()
                {
                    Id = i.WarehouseItem.Id,
                    Name = i.WarehouseItem.Name,
                    Code = i.WarehouseItem.Code,
                    Active = i.WarehouseItem.Active,
                    MainMeasureUnitId = i.WarehouseItem.MainMeasureUnitId,
                    SecondaryMeasureUnitId = i.WarehouseItem.SecondaryMeasureUnitId,
                    BuyMeasureUnitId = i.WarehouseItem.BuyMeasureUnitId,
                    SecondaryUnitToMainRate = i.WarehouseItem.SecondaryUnitToMainRate,
                    BuyUnitToMainRate = i.WarehouseItem.BuyUnitToMainRate,
                    FpaDefId = i.WarehouseItem.FpaDefId,
                    MaterialCategoryId = i.WarehouseItem.MaterialCategoryId,
                    MaterialType = (int)i.WarehouseItem.MaterialType,
                    WarehouseItemNature = (int)i.WarehouseItem.WarehouseItemNature,
                    PriceNetto = i.WarehouseItem.PriceNetto,
                    PriceBrutto = i.WarehouseItem.PriceBrutto,  
                    ManufacturerCode = i.WarehouseItem.ManufacturerCode,
                    ModifiedAt = i.WarehouseItem.DateLastModified
                    
                })
                .ToListAsync();

            return Ok(items);
        }
        [HttpPost("AddCashierWarehouseItem")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> AddCashierWarehouseItem([FromBody] CashierItemCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            try
            {
                var warehouseItemToInsert = new WarehouseItemCreateDto()
                {
                    Active = request.Item.Active,
                    MainMeasureUnitId = request.Item.MainMeasureUnitId,
                    SecondaryMeasureUnitId = request.Item.SecondaryMeasureUnitId,
                    BuyMeasureUnitId = request.Item.BuyMeasureUnitId,
                    SecondaryUnitToMainRate = request.Item.SecondaryUnitToMainRate,
                    BuyUnitToMainRate = request.Item.BuyUnitToMainRate,
                    ManufacturerCode = request.Item.ManufacturerCode,
                    Code = request.Item.Code,
                    Name = request.Item.Name,
                    ShortDescription = request.Item.Name,
                    Description = request.Item.Name,
                    MaterialType = (MaterialTypeEnum)request.Item.MaterialType,
                    MaterialCategoryId = request.Item.MaterialCategoryId,
                    WarehouseItemNature = (WarehouseItemNatureEnum) request.Item.WarehouseItemNature,
                    
                };
                var syncServiceResult = await _warehouseManagementSrv.AddWarehouseItemAsync(warehouseItemToInsert);
                if (syncServiceResult is null)
                {
                    return StatusCode(500, new
                    {
                        error = "Internal server error occurred during business buy document synchronization"
                    });
                }

                if (!syncServiceResult.Success)
                {
                    return BadRequest(new { error = syncServiceResult.ErrorMessage });
                }

                var res = syncServiceResult.Data;
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }
        [HttpPost("SyncCheckBusinessBuyDocument")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncCheckBusinessBuyDocument([FromBody] SyncBusinessBuyDocumentRequest request)
        {
            #region Boiler Plate Code

            _logger.LogInformation("SyncCheckBusinessBuyDocument");

            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            if (string.IsNullOrEmpty(request.CompanyCode))
            {
                return BadRequest(new
                {
                    error = "No Company Code"
                });
            }

            string businessCompanyCode = request.CompanyCode;

            #endregion

            bool hasBeenSynced = false;
            bool hasSupplierBeenSynced = false;
            bool hasPayedAmountEqualWithTotalAmountOrZero = false;
            try
            {
                hasBeenSynced = _context.SyncBuyDocuments.Any(p =>
                    p.CompanyCode == businessCompanyCode && p.BusId == request.Id);
                hasSupplierBeenSynced = _context.SyncSuppliers.Any(p =>
                    p.CompanyCode == businessCompanyCode && p.BusId == request.SupplierId);
                if (request.PayedAmount == 0)
                {
                    hasPayedAmountEqualWithTotalAmountOrZero = true;
                }
                else
                {
                    decimal dif = request.PayedAmount - decimal.Abs(request.TotalAmount);
                    hasPayedAmountEqualWithTotalAmountOrZero = decimal.Abs(dif) < 0.01m;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during document sync check: {Error}", ex.Message);
                return BadRequest(new
                {
                    error = "SyncCheckBusinessBuyDocument error " + ex.Message
                });
            }

            string message = "Unknown condition";
            if (hasBeenSynced)
            {
                message = "Document is synced";
            }
            else
            {
                message = "Document is not synced-";
                if (hasSupplierBeenSynced)
                {
                    message += "Supplier is synced-";
                }
                else
                {
                    message += "Supplier is not synced-";
                }

                if (hasPayedAmountEqualWithTotalAmountOrZero)
                {
                    message += "Payed amount Ok";
                }
                else
                {
                    message += "Payed amount is not equal with total amount";
                }
            }

            var res = new ErpCheckDocumentResponse()
            {
                Message = message,
                IsSynced = hasBeenSynced,
                CanSync = !hasBeenSynced && hasSupplierBeenSynced && hasPayedAmountEqualWithTotalAmountOrZero,
                DocumentId = request.Id
            };
            return Ok(res);
        }

        [HttpPost("SyncAddBusinessBuyDocument")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncAddBusinessBuyDocument([FromBody] SyncBusinessBuyDocumentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }

            try
            {
                var syncServiceResult = await _docSyncSrv.SyncAddBusinessBuyDocument(request);
                if (syncServiceResult is null)
                {
                    return StatusCode(500, new
                    {
                        error = "Internal server error occurred during business buy document synchronization"
                    });
                }

                if (!syncServiceResult.Success)
                {
                    return BadRequest(new { error = syncServiceResult.ErrorMessage });
                }

                var res = syncServiceResult.Data;
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        [HttpPost("SyncAddBusinessBuyDocuments")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncAddBusinessBuyDocuments([FromBody]SyncBusinessEntityRequest<SyncBusinessBuyDocumentRequest> request)
        {
            #region Error Checking

            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }
            if (request.Items == null)
            {
                return BadRequest(new
                {
                    error = "No Documents to sync"
                });
            }
            if (request.Items.Count == 0)
            {
                return BadRequest(new
                {
                    error = "List has no documents to sync"
                });
            }
            #endregion
            #region Boiler Plate Code
            _logger.LogInformation("SyncBuyDocuments");
            int addedCount = 0;
            int failedToAddCount = 0;
            int updatedCount = 0;
            int failedToUpdateCount = 0;
            int deletedCount = 0;
            int failedToDeleteCount = 0;
            Guid syncSessionId = Guid.NewGuid(); 
            #endregion
            foreach (var item in request.Items)
            {
                try
                {
                    item.CompanyCode = request.CompanyCode;
                    var syncServiceResult = await _docSyncSrv.SyncAddBusinessBuyDocument(item, syncSessionId);;
                    if (syncServiceResult is null)
                    {
                       failedToAddCount++;
                       continue;
                    }
                    if (!syncServiceResult.Success)
                    {
                       failedToAddCount++;
                       continue;
                    }
                    addedCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _logger.LogError("An error occurred during document sync: {Error}", ex.Message);
                    failedToAddCount++;
                }
                
            }
            var res = new ErpSynchronizationResponse<SyncBuyDocument>
            {
                Message = "SyncBuyDocuments",
                AddedCount = addedCount,
                FailedToAddCount = failedToAddCount,
                UpdatedCount = updatedCount,
                FailedToUpdateCount = failedToUpdateCount,
                DeletedCount = deletedCount,
                FailedToDeleteCount = failedToDeleteCount, 
                SyncSessionId = syncSessionId,
                //SyncSource = syncSource,
                //SyncItems = toInsert.Concat(toUpdate).Concat(toDelete).ToList()
            };
            return Ok(res);
        }

        [HttpPost("SyncAddBusinessDayCloseData")]
        [Authorize(Policy = "ApiPolicy2")]
        public async Task<IActionResult> SyncAddBusinessDayCloseData([FromBody] DayClosePayload request)
        {
            #region Error Checking

            if (request == null)
            {
                return BadRequest(new
                {
                    error = "Empty request data"
                });
            }
           
            #endregion

            try
            {
                var syncServiceResult = await _docSyncSrv.SyncAddDayCloseData(request, null);;
                if (syncServiceResult is null)
                {
                    return BadRequest(new
                    {
                        error = "Empty response from sync service"
                    });
                }
                if (!syncServiceResult.Success)
                {
                    return BadRequest(new
                    {
                        error = syncServiceResult.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError("An error occurred during document sync: {Error}", ex.Message);
                var dayCloseResult = new DayCloseResponse()
                {
                    Message = "Το κλείσιμο ημέρας ενημερώθηκε με επιτυχία." + ex.Message,
                    IsSuccess = true
                };
                return BadRequest(dayCloseResult);
            }
            var res = new DayCloseResponse()
            {
                Message = "Το κλείσιμο ημέρας ενημερώθηκε με επιτυχία.",
                IsSuccess = true
            };
            return Ok(res);
        }
    }
}