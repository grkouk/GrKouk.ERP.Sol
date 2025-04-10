using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// Request for syncing entity of type T
/// </summary>
/// <typeparam name="T"></typeparam>
public class SyncBusinessRequest<T>
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    private IList<T> _items;
    
    [JsonPropertyName("items")]
    public IList<T>  Items
    {
        get { return _items ??= new List<T>() ; }
        set => _items = value;
    }
}