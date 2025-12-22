using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

public class CashierItemCreateRequest 
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierItemCreateDto Item { get; set; }
 
}