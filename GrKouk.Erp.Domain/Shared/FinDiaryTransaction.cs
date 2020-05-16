using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Financial Diary Transaction
    /// </summary>
    public class FinDiaryTransaction
    {
        public int Id { get; set; }
        /// <summary>
        /// Ημερομηνία Κίνησης
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }
        /// <summary>
        /// Αριθμός Παραστατικού
        /// </summary>

        [MaxLength(50)]
        public string ReferenceCode { get; set; }
        [Required]
        public int TransactorId { get; set; }
        public Transactor Transactor { get; set; }

        public int FinTransCategoryId { get; set; }
        public FinTransCategory FinTransCategory { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int CostCentreId { get; set; }
        public CostCentre CostCentre { get; set; }

        public int RevenueCentreId { get; set; }
        public RevenueCentre RevenueCentre { get; set; }

        /// <summary>
        /// Αιτιολογία Κίνησης
        /// </summary>
        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        /// <summary>
        /// Transaction Kind
        /// 1=Exprence
        /// 2=Income
        /// </summary>
        public int Kind { get; set; }

        /// <summary>
        /// Ποσό ΦΠΑ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountFpa { get; set; }
        /// <summary>
        /// Καθαρό Ποσό
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountNet { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}