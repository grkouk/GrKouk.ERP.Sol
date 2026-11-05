using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// One shop's answer about one myDATA document ("I have it", "it isn't mine", ...). Part of the shared
/// reconciliation memory: every cash register fetches the full myDATA feed itself and publishes what it
/// knows here, so each shop can see all shops' answers. The ERP never talks to AADE.
///
/// Rows are replaced, never edited: a publish deletes that company's rows of the period and inserts the
/// new list, so a deleted or moved document cannot leave an old answer behind.
/// </summary>
public class MyDataShopClaim
{
    /// <summary>The ERP company (branch) of the shop that gave this answer.</summary>
    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    /// <summary>myDATA MARK. Unique per document, and the same in every shop's feed.</summary>
    public long Mark { get; set; }

    /// <summary>The answer, one of the MyDataClaimKinds constants.</summary>
    [Required]
    [MaxLength(20)]
    public string ClaimKind { get; set; }

    [MaxLength(20)]
    public string IssuerVat { get; set; }

    [MaxLength(50)]
    public string Series { get; set; }

    [MaxLength(50)]
    public string Aa { get; set; }

    public DateTime IssueDate { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal MyDataGross { get; set; }

    /// <summary>The shop's own check of the document against its purchase, as text (Exact, Explained, ...).</summary>
    [MaxLength(20)]
    public string ReconStatus { get; set; }

    /// <summary>The shop's reconciliation cause flags. The ERP stores them and does not read them.</summary>
    public int ReconCauses { get; set; }

    public Guid? LocalBuyDocumentId { get; set; }

    [MaxLength(50)]
    public string LocalRefNumber { get; set; }

    public DateTime? LocalDate { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? LocalGross { get; set; }

    /// <summary>The purchase's net and VAT per rate, as JSON. Only read back whole, never queried.</summary>
    public string LocalRatesJson { get; set; }

    public DateTime? LocalContentModifiedAt { get; set; }

    /// <summary>Upload state as the shop sees it (NotUploaded, Uploaded, ChangedSinceUpload).</summary>
    [MaxLength(20)]
    public string UploadState { get; set; }

    public int? ErpBuyDocId { get; set; }

    /// <summary>The purchase is linked here, but the delivery address names another shop.</summary>
    public bool AddressConflict { get; set; }

    [MaxLength(500)]
    public string AcceptedNote { get; set; }

    [MaxLength(100)]
    public string AcceptedBy { get; set; }

    public DateTime? AcceptedAt { get; set; }

    /// <summary>Server time (UTC) of the publish that wrote this row.</summary>
    public DateTime PublishedAt { get; set; }
}

/// <summary>
/// When a shop last published a period. Without a row covering a period, that shop's answers are
/// unknown: it must be shown as "unknown", never as "missing".
/// </summary>
public class MyDataShopPublish
{
    public int Id { get; set; }

    [Required]
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    public DateTime PeriodFrom { get; set; }

    public DateTime PeriodTo { get; set; }

    public DateTime PublishedAt { get; set; }

    [MaxLength(50)]
    public string ClientVersion { get; set; }

    /// <summary>The number of answers in that publish.</summary>
    public int Count { get; set; }
}

/// <summary>
/// A decision about every document of one issuer (ErpOnly or Ignore), shared by all shops. An issuer
/// without a row is handled normally. Last writer wins, which is acceptable for such rare edits.
/// </summary>
public class MyDataIssuerRule
{
    /// <summary>ΑΦΜ as the comparison key: digits only, no EL/GR prefix, no leading zeros.</summary>
    [Key]
    [MaxLength(20)]
    public string IssuerVat { get; set; }

    [Required]
    [MaxLength(20)]
    public string Action { get; set; }

    [MaxLength(500)]
    public string Note { get; set; }

    /// <summary>The cash register's login user. The API token itself is shared, so it cannot tell.</summary>
    [MaxLength(100)]
    public string By { get; set; }

    public DateTime At { get; set; }
}
