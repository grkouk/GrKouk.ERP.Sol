using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class ErpSupplierDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("afm")]
    public string Afm { get; set; }

}