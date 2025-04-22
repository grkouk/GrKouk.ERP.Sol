using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBusinessSupplierDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("taxNumber")]
    public string TaxNumber { get; set; }
}

public class SyncSupplierDto
{
    public Guid Id { get; set; }
    public int BusId { get; set; }
  
    public string CompanyCode { get; set; }
    public string BusCode { get; set; }
    public string Name { get; set; }
    public int ErpId { get; set; }
    public string TaxNumber { get; set; }
    public string SourceChecksum { get; set; }
}