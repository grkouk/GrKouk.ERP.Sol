using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashRegister
{
    /// <summary>A shop's answer about one myDATA document.</summary>
    public static class MyDataClaimKinds
    {
        /// <summary>Linked to a local purchase.</summary>
        public const string Entered = "Entered";
        /// <summary>Routed to this shop, not entered.</summary>
        public const string Missing = "Missing";
        /// <summary>Routed to another shop by the delivery address.</summary>
        public const string NotMine = "NotMine";
        /// <summary>No delivery address, not entered here.</summary>
        public const string Unrouted = "Unrouted";
        /// <summary>Zero-value 9.3 delivery note, cancelled in myDATA, or the issuer rule says Ignore.</summary>
        public const string Ignored = "Ignored";
        /// <summary>An expense entered only in the ERP (set by hand, or by an issuer rule).</summary>
        public const string ErpOnly = "ErpOnly";

        public static bool IsValid(string kind) =>
            kind is Entered or Missing or NotMine or Unrouted or Ignored or ErpOnly;
    }

    /// <summary>The ERP's comparison of its own copy with the shop's amounts, for an Entered answer.</summary>
    public static class MyDataErpCheckResults
    {
        public const string Ok = "Ok";
        public const string Differs = "Differs";
        public const string Missing = "Missing";
        public const string NotUploaded = "NotUploaded";
    }

    /// <summary>The ERP's search for a document said to be entered only in the ERP.</summary>
    public static class MyDataErpOnlyResults
    {
        public const string Found = "Found";
        /// <summary>The supplier exists in the ERP, but has no document with this number near this date.</summary>
        public const string NotFound = "NotFound";
        /// <summary>No ERP supplier has this ΑΦΜ, so the search could not run. Not proof of a missing document.</summary>
        public const string NoSupplier = "NoSupplier";
    }

    /// <summary>What to do with every document of an issuer. Normal removes the issuer's rule.</summary>
    public static class MyDataIssuerActions
    {
        public const string Normal = "Normal";
        public const string ErpOnly = "ErpOnly";
        public const string Ignore = "Ignore";

        public static bool IsValid(string action) => action is Normal or ErpOnly or Ignore;
    }

    /// <summary>Net and VAT of one VAT rate. <see cref="Rate"/> is a fraction: 24% is 0.24. Net is after discount.</summary>
    public class MyDataClaimRateDto
    {
        public decimal Rate { get; set; }
        public decimal Net { get; set; }
        public decimal Vat { get; set; }
    }

    public class MyDataClaimDto
    {
        public long Mark { get; set; }

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
        public decimal MyDataGross { get; set; }

        [MaxLength(20)]
        public string ReconStatus { get; set; }

        public int ReconCauses { get; set; }

        // Entered only: the local purchase.
        public Guid? LocalBuyDocumentId { get; set; }

        [MaxLength(50)]
        public string LocalRefNumber { get; set; }

        public DateTime? LocalDate { get; set; }
        public decimal? LocalGross { get; set; }
        public List<MyDataClaimRateDto> LocalRates { get; set; }
        public DateTime? LocalContentModifiedAt { get; set; }

        [MaxLength(20)]
        public string UploadState { get; set; }

        public int? ErpBuyDocId { get; set; }
        public bool AddressConflict { get; set; }

        [MaxLength(500)]
        public string AcceptedNote { get; set; }

        [MaxLength(100)]
        public string AcceptedBy { get; set; }

        public DateTime? AcceptedAt { get; set; }
    }

    /// <summary>
    /// A shop's full answer list for a period. It replaces what that company published before for the
    /// period, so it must hold every document of the period, and only those.
    /// </summary>
    public class MyDataPublishRequestDto
    {
        [Required]
        [MaxLength(15)]
        public string CompanyCode { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        [MaxLength(50)]
        public string ClientVersion { get; set; }

        [Required]
        public List<MyDataClaimDto> Claims { get; set; }
    }

    public class MyDataPublishResultDto
    {
        public DateTime PublishedAt { get; set; }
        public int Count { get; set; }
    }

    /// <summary>An answer as stored, plus the ERP's own check of its copy (Entered answers only).</summary>
    public class MyDataStoredClaimDto : MyDataClaimDto
    {
        public string CompanyCode { get; set; }
        public DateTime PublishedAt { get; set; }

        /// <summary>A MyDataErpCheckResults constant. Null unless the answer is Entered.</summary>
        public string ErpCheck { get; set; }

        /// <summary>The ERP document the check read. May come from the ERP's own upload record.</summary>
        public int? ErpCheckedBuyDocId { get; set; }

        /// <summary>The company that holds the ERP copy (a copy in another company counts as Differs).</summary>
        public string ErpCompanyCode { get; set; }

        public decimal? ErpGross { get; set; }

        /// <summary>The ERP copy's net and VAT per rate, summed from its lines.</summary>
        public List<MyDataClaimRateDto> ErpRates { get; set; }
    }

    public class MyDataPublishInfoDto
    {
        public string CompanyCode { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public DateTime PublishedAt { get; set; }
        public string ClientVersion { get; set; }
        public int Count { get; set; }
    }

    public class MyDataIssuerRuleDto
    {
        /// <summary>
        /// ΑΦΜ. Any form on the way in; on the way out always the comparison key: digits only, no
        /// EL/GR prefix, no leading zeros.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string IssuerVat { get; set; }

        /// <summary>A MyDataIssuerActions constant. Normal removes the rule.</summary>
        [Required]
        [MaxLength(20)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [MaxLength(100)]
        public string By { get; set; }

        /// <summary>Set by the server.</summary>
        public DateTime At { get; set; }
    }

    /// <summary>The ERP's search for one document that a shop or an issuer rule calls ERP-only.</summary>
    public class MyDataErpOnlyCheckDto
    {
        public long Mark { get; set; }

        /// <summary>A MyDataErpOnlyResults constant.</summary>
        public string Result { get; set; }

        // Found only. The search covers every ERP company.
        public int? ErpBuyDocId { get; set; }
        public string CompanyCode { get; set; }
        public string RefCode { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Gross { get; set; }
    }

    /// <summary>A purchase of a company that has not published the period: a hint, not an answer.</summary>
    public class MyDataErpHeaderDto
    {
        public int ErpBuyDocId { get; set; }
        public string CompanyCode { get; set; }
        public DateTime Date { get; set; }
        public string RefCode { get; set; }
        public string SupplierVat { get; set; }
        public decimal Gross { get; set; }
    }

    /// <summary>One supplier's purchase documents of a period, pooled across the selected companies.</summary>
    public class MyDataErpSupplierTotalDto
    {
        public int TransactorId { get; set; }
        public string TaxNumber { get; set; }
        public string Name { get; set; }
        public int Documents { get; set; }

        /// <summary>Invoices minus credit notes, by each document type's own debit or credit setting.</summary>
        public decimal Gross { get; set; }

        /// <summary>
        /// Its documents. Many old ERP suppliers have no ΑΦΜ, so the app recognises them by these: the
        /// same numbers and amounts as a myDATA issuer's documents mean the same supplier.
        /// </summary>
        public List<MyDataErpSupplierDocDto> Docs { get; set; } = new();
    }

    public class MyDataErpSupplierDocDto
    {
        public string RefCode { get; set; }
        public DateTime Date { get; set; }
        public decimal Gross { get; set; }
    }

    public class MyDataClaimsResponseDto
    {
        public List<MyDataStoredClaimDto> Claims { get; set; } = new();

        /// <summary>Publishes that overlap the requested period.</summary>
        public List<MyDataPublishInfoDto> Publishes { get; set; } = new();

        /// <summary>Every issuer rule. These are not tied to a period.</summary>
        public List<MyDataIssuerRuleDto> IssuerRules { get; set; } = new();

        public List<MyDataErpOnlyCheckDto> ErpOnlyChecks { get; set; } = new();

        /// <summary>Purchase headers of the companies with no publish covering the whole period.</summary>
        public List<MyDataErpHeaderDto> FallbackHeaders { get; set; } = new();
    }
}
