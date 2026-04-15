using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

/// <summary>
/// Links a client-side BuyDocument (Guid) to the ERP BuyDocument (int) it was uploaded as.
/// Used for idempotent retries: if the client re-sends an upload after a lost response,
/// the ERP can return the existing ErpBuyDocId instead of creating a duplicate.
/// </summary>
public class UploadedBuyDocument
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The client's local BuyDocument.Id (Guid). Unique — one tracking row per local doc.
    /// </summary>
    [Required]
    public Guid LocalBuyDocumentId { get; set; }

    /// <summary>
    /// The ERP-side BuyDocument.Id created for this upload.
    /// </summary>
    [Required]
    public int ErpBuyDocId { get; set; }

    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    public DateTime UploadedAt { get; set; }
}
