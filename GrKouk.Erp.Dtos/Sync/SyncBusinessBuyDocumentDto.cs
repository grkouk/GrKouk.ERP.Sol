using System;
using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBusinessBuyDocumentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }
    [JsonPropertyName("refNumber")]
    public int RefNumber { get; set; }
  
}