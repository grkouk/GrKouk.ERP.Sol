using System.Collections.Generic;
using System.Text.Json.Serialization;
using GrKouk.Erp.Dtos.Sync;

namespace GrKouk.Web.ERP.Helpers;

public class SyncBusinessBuyDocumentsRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    private IList<SyncBusinessBuyDocumentDto> _items;
    [JsonPropertyName("items")]
    public IList<SyncBusinessBuyDocumentDto> Items
    {
        get { return _items ??= new List<SyncBusinessBuyDocumentDto>() ; }
        set => _items = value;
    }
}