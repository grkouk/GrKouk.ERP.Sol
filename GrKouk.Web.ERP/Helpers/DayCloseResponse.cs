using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;


public class DayCloseResponse
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = false;
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
  
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; } = "";
}