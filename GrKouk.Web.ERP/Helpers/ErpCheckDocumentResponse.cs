using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

public class ErpCheckDocumentResponse
{
    [JsonPropertyName("isSynced")] 
    public bool IsSynced { get; set; } = false;
    [JsonPropertyName("isChanged")] 
    public bool IsChanged { get; set; } = false;

    [JsonPropertyName("canSync")] 
    public bool CanSync { get; set; } = false;
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("documentId")]
    public int DocumentId { get; set; }
}