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
public class CashierItemModifyRequest 
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierItemModifyDto Item { get; set; }
 
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
    public int DaysOverdue { get; set; }
    public DateTime ModifiedAt { get; set; }
}
public class CashierItemCategoryModifyRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierItemCategoryModifyDto Item { get; set; }
}
public class CashierItemCategoryModifyDto
{
    public int Id { get; set; }
      
    public string Code { get; set; }
       
    public string Name { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime ModifiedAt { get; set; }
}
public class CashierPaymentMethodCreateRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierPaymentMethodCreateDto Item { get; set; }
}
public class CashierPaymentMethodCreateDto
{
    public int Id { get; set; }
      
    public string Code { get; set; }
       
    public string Name { get; set; }
    public DateTime ModifiedAt { get; set; }
}
public class CashierPaymentMethodModifyRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("item")]
    public CashierPaymentMethodModifyDto Item { get; set; }
}
public class CashierPaymentMethodModifyDto
{
    public int Id { get; set; }
      
    public string Code { get; set; }
       
    public string Name { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime ModifiedAt { get; set; }
}
public class ErpPaymentMethodDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime ModifiedAt { get; set; }
  
}