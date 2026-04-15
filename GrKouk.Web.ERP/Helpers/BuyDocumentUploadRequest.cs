using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// Cash-register upload payload for a BuyDocument. The client pre-resolves
/// all ERP ids (supplier, payment method, doc series, items). The ERP resolves
/// only CompanyId from CompanyCode and hands the rest straight to
/// IDocumentTransactionService.AddBuyDocument.
/// </summary>
public class BuyDocumentUploadRequest
{
    [JsonPropertyName("localBuyDocumentId")]
    [Required]
    public Guid LocalBuyDocumentId { get; set; }

    /// <summary>
    /// If set, delete this ERP BuyDocument before creating the new one
    /// (delete-and-replace re-upload).
    /// </summary>
    [JsonPropertyName("existingErpBuyDocId")]
    public int? ExistingErpBuyDocId { get; set; }

    [JsonPropertyName("companyCode")]
    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    [JsonPropertyName("transDate")]
    [Required]
    public DateTime TransDate { get; set; }

    [JsonPropertyName("transRefCode")]
    public string? TransRefCode { get; set; }

    [JsonPropertyName("transactorId")]
    [Required]
    public int TransactorId { get; set; }

    [JsonPropertyName("buyDocSeriesId")]
    [Required]
    public int BuyDocSeriesId { get; set; }

    [JsonPropertyName("paymentMethodId")]
    public int PaymentMethodId { get; set; }

    [JsonPropertyName("amountNet")]
    public decimal AmountNet { get; set; }

    [JsonPropertyName("amountFpa")]
    public decimal AmountFpa { get; set; }

    [JsonPropertyName("amountDiscount")]
    public decimal AmountDiscount { get; set; }

    [JsonPropertyName("etiology")]
    [MaxLength(500)]
    public string? Etiology { get; set; }

    [JsonPropertyName("lines")]
    public List<BuyDocumentUploadLine> Lines { get; set; } = new();
}

public class BuyDocumentUploadLine
{
    [JsonPropertyName("warehouseItemId")]
    [Required]
    public int WarehouseItemId { get; set; }

    [JsonPropertyName("mainUnitId")]
    public int MainUnitId { get; set; }

    [JsonPropertyName("secUnitId")]
    public int SecUnitId { get; set; }

    [JsonPropertyName("transactionUnitId")]
    public int TransactionUnitId { get; set; }

    [JsonPropertyName("transactionUnitFactor")]
    public float TransactionUnitFactor { get; set; } = 1f;

    [JsonPropertyName("factor")]
    public float Factor { get; set; } = 1f;

    [JsonPropertyName("transactionQuantity")]
    public double TransactionQuantity { get; set; }

    [JsonPropertyName("q1")]
    public double Q1 { get; set; }

    [JsonPropertyName("q2")]
    public double Q2 { get; set; }

    [JsonPropertyName("transUnitPrice")]
    public decimal TransUnitPrice { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("amountDiscount")]
    public decimal AmountDiscount { get; set; }

    [JsonPropertyName("amountExpenses")]
    public decimal AmountExpenses { get; set; }

    [JsonPropertyName("discountRate")]
    public float DiscountRate { get; set; }

    [JsonPropertyName("fpaRate")]
    public float FpaRate { get; set; }
}
