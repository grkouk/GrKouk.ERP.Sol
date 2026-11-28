using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// Cash-register payload for the V2 day-close. Each Document is one bucket
/// (ErpSellDocSeriesId, ErpPaymentMethodId, IsStar) carrying lines aggregated
/// by ErpItemId. SubmissionId enforces server-side idempotency so a retry
/// after a transient failure cannot double-book.
/// </summary>
public class DayCloseUploadRequest
{
    [JsonPropertyName("submissionId")]
    [Required]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("companyCode")]
    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; } = string.Empty;

    [JsonPropertyName("transDate")]
    [Required]
    public DateTime TransDate { get; set; }

    [JsonPropertyName("zNumber")]
    [Required]
    public int ZNumber { get; set; }

    [JsonPropertyName("documents")]
    public List<DayCloseUploadDocument> Documents { get; set; } = new();
}

public class DayCloseUploadDocument
{
    [JsonPropertyName("erpSellDocSeriesId")]
    [Required]
    public int ErpSellDocSeriesId { get; set; }

    [JsonPropertyName("erpPaymentMethodId")]
    [Required]
    public int ErpPaymentMethodId { get; set; }

    [JsonPropertyName("isStar")]
    public bool IsStar { get; set; }

    /// <summary>
    /// "cash" / "card" / "star" — drives TransRefCode suffix (CH/CD/ST). "other" if unrecognized.
    /// </summary>
    [JsonPropertyName("paymentKind")]
    public string PaymentKind { get; set; } = "other";

    [JsonPropertyName("sourceDocumentCount")]
    public int SourceDocumentCount { get; set; }

    /// <summary>
    /// Platform group (sales an order platform such as efood collected): the ERP customer the
    /// document posts to, instead of the retail customer. Honoured by SyncUploadDayCloseV3 only;
    /// V2 refuses a request that carries one, so it can never land on the retail customer.
    /// </summary>
    [JsonPropertyName("erpTransactorId")]
    public int? ErpTransactorId { get; set; }

    /// <summary>TransRefCode suffix for a platform group ("EF" → Z702-EF). Empty = from PaymentKind.</summary>
    [JsonPropertyName("transRefMarker")]
    [MaxLength(4)]
    public string? TransRefMarker { get; set; }

    [JsonPropertyName("amountNet")]
    public decimal AmountNet { get; set; }

    [JsonPropertyName("amountFpa")]
    public decimal AmountFpa { get; set; }

    [JsonPropertyName("amountBrut")]
    public decimal AmountBrut { get; set; }

    [JsonPropertyName("lines")]
    public List<DayCloseUploadLine> Lines { get; set; } = new();
}

public class DayCloseUploadLine
{
    [JsonPropertyName("warehouseItemId")]
    [Required]
    public int WarehouseItemId { get; set; }

    [JsonPropertyName("fpaRate")]
    public float FpaRate { get; set; }

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; } = 1d;

    [JsonPropertyName("amountNet")]
    public decimal AmountNet { get; set; }

    [JsonPropertyName("amountFpa")]
    public decimal AmountFpa { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public class DayCloseUploadResponse
{
    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("erpSellDocIds")]
    public List<int> ErpSellDocIds { get; set; } = new();
}
