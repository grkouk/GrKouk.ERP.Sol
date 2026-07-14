using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

/// <summary>
/// Links a client-side SellDocument (Guid) to the ERP SellDocument (int) it was uploaded as
/// through the per-document "ledger sell" path (e.g. supplier bonus credit). Mirror of
/// <see cref="UploadedBuyDocument"/>. Used for idempotent retries: if the client re-sends an
/// upload after a lost response, the ERP can return the existing ErpSellDocId instead of
/// creating a duplicate.
/// </summary>
public class UploadedSellDocument
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The client's local SellDocument.Id (Guid). Unique — one tracking row per local doc.
    /// </summary>
    [Required]
    public Guid LocalSellDocumentId { get; set; }

    /// <summary>
    /// The ERP-side SellDocument.Id created for this upload.
    /// </summary>
    [Required]
    public int ErpSellDocId { get; set; }

    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    public DateTime UploadedAt { get; set; }
}
