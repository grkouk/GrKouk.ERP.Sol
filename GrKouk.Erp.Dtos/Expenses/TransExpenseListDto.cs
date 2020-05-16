using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Dtos.Expenses
{
    public class TransExpenseListDto
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
        public string TransactorName { get; set; }

        public int FinTransCategoryId { get; set; }
        
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }

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
        public decimal Rate { get; set; }
        public decimal DisplayNetAmount => AmountNet * Rate;
        public decimal DisplayFpaAmount => AmountFpa * Rate;
    }
}
