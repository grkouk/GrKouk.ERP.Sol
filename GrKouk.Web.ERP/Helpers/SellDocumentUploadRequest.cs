using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// Cash-register upload payload for a per-document "ledger sell" (e.g. supplier bonus credit).
/// The client pre-resolves all ERP ids (transactor, payment method, sell doc series, items,
/// units). The ERP resolves only CompanyId from CompanyCode and a default SalesChannel, then
/// hands the rest to IDocumentTransactionService.AddSalesDoc. Mirror of
/// <see cref="BuyDocumentUploadRequest"/>.
/// </summary>
public class SellDocumentUploadRequest
{
    [JsonPropertyName("localSellDocumentId")]
    [Required]
    public Guid LocalSellDocumentId { get; set; }

    /// <summary>
    /// If set, delete this ERP SellDocument before creating the new one
    /// (delete-and-replace re-upload).
    /// </summary>
    [JsonPropertyName("existingErpSellDocId")]
    public int? ExistingErpSellDocId { get; set; }

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

    [JsonPropertyName("sellDocSeriesId")]
    [Required]
    public int SellDocSeriesId { get; set; }

    [JsonPropertyName("paymentMethodId")]
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Optional. The cash register has no sales-channel concept, so this is normally 0; the
    /// upload service then resolves a default SalesChannel (SellDocument.SalesChannel is a
    /// required FK on the ERP side).
    /// </summary>
    [JsonPropertyName("salesChannelId")]
    public int SalesChannelId { get; set; }

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
    public List<SellDocumentUploadLine> Lines { get; set; } = new();
}

public class SellDocumentUploadLine
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
