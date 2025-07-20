using System;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

public class DayClosePayload
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("zNumber")]
    public int ZNumber { get; set; }
    [JsonPropertyName("totalCash")]
    public decimal TotalCash { get; set; } 
    [JsonPropertyName("totalCards")]
    public decimal TotalCards { get; set; }
    [JsonPropertyName("totalStar")]
    public decimal TotalStar { get; set; }
}