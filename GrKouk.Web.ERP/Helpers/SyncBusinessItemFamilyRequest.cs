using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GrKouk.Erp.Domain.Sync;
using GrKouk.Erp.Dtos.Sync;

namespace GrKouk.Web.ERP.Helpers;

public class SyncBusinessItemFamilyRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    private IList<SyncBusinessFamilyItemDto> _items;
    [JsonPropertyName("items")]
    public IList<SyncBusinessFamilyItemDto> Items
    {
        get { return _items ??= new List<SyncBusinessFamilyItemDto>() ; }
        set => _items = value;
    }
}

public class SyncBusinessUnitsOfMeasurementRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    private IList<SyncBusinessFamilyItemDto> _items;
    [JsonPropertyName("items")]
    public IList<SyncBusinessFamilyItemDto> Items
    {
        get { return _items ??= new List<SyncBusinessFamilyItemDto>() ; }
        set => _items = value;
    }
}
public class ErpSynchronizationResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("addedCount")]
    public int AddedCount { get; set; }
    [JsonPropertyName("FailedToAddCount")]
    public int FailedToAddCount { get; set; }
    [JsonPropertyName("updatedCount")]
    public int UpdatedCount { get; set; }
    [JsonPropertyName("FailedToUpdateCount")]
    public int FailedToUpdateCount { get; set; }
    [JsonPropertyName("deletedCount")]
    public int DeletedCount { get; set; }
    [JsonPropertyName("FailedToDeleteCount")]
    public int FailedToDeleteCount { get; set; }
    [JsonPropertyName("syncSessionId")]
    public Guid SyncSessionId { get; set; }
    [JsonPropertyName("syncSource")]
    public string SyncSource { get; set; }
    [JsonPropertyName("syncItems")]
    public List<T> SyncItems { get; set; }
}