using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

/// <summary>
/// Tracks a cash-register V2 day-close submission and the ERP SellDocument ids
/// it produced. Used for idempotent retries: if the client re-sends the same
/// SubmissionId after a lost response, the ERP returns the original ids
/// instead of creating duplicates.
/// </summary>
public class UploadedDayCloseSubmission
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Client-generated submission id. Unique — one tracking row per submission.
    /// </summary>
    [Required]
    public Guid SubmissionId { get; set; }

    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    public int ZNumber { get; set; }

    /// <summary>
    /// Comma-separated ERP SellDocument ids created for this submission, in
    /// payload order. Plain CSV keeps storage simple — we never query by id.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string ErpSellDocIdsCsv { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
