using System.Collections.Generic;
using System.Text.Json.Serialization;
using GrKouk.Erp.Dtos.Sync;

namespace GrKouk.Web.ERP.Helpers;

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