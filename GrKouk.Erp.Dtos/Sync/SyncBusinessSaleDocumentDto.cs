using System;
using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBusinessSaleDocumentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }
    [JsonPropertyName("refNumber")]
    public int RefNumber { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
    [JsonPropertyName("payedAmount")]
    public decimal PayedAmount { get; set; }
}