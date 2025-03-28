using System.Text.Json.Serialization;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBusinessFamilyItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class SyncBusinessUnitOfMeasurementDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}