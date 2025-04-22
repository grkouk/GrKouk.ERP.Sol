using System.Collections.Generic;
using System.Text.Json.Serialization;
using GrKouk.Erp.Dtos.Sync;

namespace GrKouk.Web.ERP.Helpers;

public class SyncBusinessSaleDocumentRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    private IList<SyncBusinessSaleDocumentDto> _items;
    [JsonPropertyName("items")]
    public IList<SyncBusinessSaleDocumentDto> Items
    {
        get { return _items ??= new List<SyncBusinessSaleDocumentDto>() ; }
        set => _items = value;
    }
}