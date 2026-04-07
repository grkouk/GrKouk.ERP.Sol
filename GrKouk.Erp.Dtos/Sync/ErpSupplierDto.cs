using System;
using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class ErpSupplierDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("taxNumber")]
    public string? TaxNumber { get; set; }
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    [JsonPropertyName("city")]
    public string? City { get; set; }
    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
    [JsonPropertyName("phoneWork")]
    public string? PhoneWork { get; set; }
    [JsonPropertyName("phoneMobile")]
    public string? PhoneMobile { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; }
}
