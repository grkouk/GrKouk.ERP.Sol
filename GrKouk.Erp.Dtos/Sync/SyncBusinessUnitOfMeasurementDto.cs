using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBusinessUnitOfMeasurementDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}