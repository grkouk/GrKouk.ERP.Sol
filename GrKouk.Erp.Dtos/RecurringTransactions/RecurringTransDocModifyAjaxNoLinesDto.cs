using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringTransDocModifyAjaxNoLinesDto
    {
        public int Id { get; set; }
        [MaxLength(2)]
        public string RecurringFrequency { get; set; }
        public RecurringDocTypeEnum RecurringDocType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NextTransDate { get; set; }

        public string TransRefCode { get; set; }
        [Required]
        public int TransactorId { get; set; }
        public int SectionId { get; set; }
        [Required]
        [Display(Name = "Doc Series")]
        public int DocSeriesId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
      
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}