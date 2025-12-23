using System;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

public class CashierItemCreateRequest 
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierItemCreateDto Item { get; set; }
 
}

public class CashierItemCategoryCreateRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierItemCategoryCreateDto Item { get; set; }
}

public class CashierItemCategoryCreateDto
{
    public int Id { get; set; }
      
    public string Code { get; set; }
       
    public string Name { get; set; }
    public DateTime ModifiedAt { get; set; }
}