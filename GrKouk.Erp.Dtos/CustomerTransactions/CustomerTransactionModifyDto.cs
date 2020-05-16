using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CustomerTransactions
{
    public class CustomerTransactionModifyDto
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public int TransCustomerDocSeriesId { get; set; }

        public int TransCustomerDocTypeId { get; set; }

        public string TransRefCode { get; set; }

        public int SectionId { get; set; }


        public int CustomerId { get; set; }

        public int FiscalPeriodId { get; set; }
        public FinancialTransactionTypeEnum TransactionType { get; set; }

        public FinActionsEnum FinancialAction { get; set; }

        [Display(Name = "VAT%")]
        public Single FpaRate { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "VAT Amount")]
        public decimal AmountFpa { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Sum Amount")]
        public decimal AmountSum => (AmountNet + AmountFpa - AmountDiscount);

        [MaxLength(500)]
        public string Etiology { get; set; }

        public int CompanyId { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
