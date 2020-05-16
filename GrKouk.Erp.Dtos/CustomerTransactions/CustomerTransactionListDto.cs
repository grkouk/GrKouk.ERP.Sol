using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CustomerTransactions
{
    public class CustomerTransactionListDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }

        public int TransCustomerDocSeriesId { get; set; }
        public string TransCustomerSeriesName { get; set; }
        [Display(Name = "Series Code")]
        public string TransCustomerSeriesCode { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }

        [Display(Name = "Section")]
        public string SectionCode { get; set; }

        public int CustomerId { get; set; }
        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        public FinancialTransactionTypeEnum TransactionType { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        
        public decimal TotalAmount
        {
            get => AmountNet+AmountFpa-AmountDiscount;

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Debit")]
        public decimal DebitAmount
        {
            get => (TransactionType.Equals(FinancialTransactionTypeEnum.FinancialTransactionTypeDebit) ?  TotalAmount:0);

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Credit")]
        public decimal CreditAmount => (TransactionType.Equals(FinancialTransactionTypeEnum.FinancialTransactionTypeCredit)
                    ? TotalAmount
                    : 0);
        [Display(Name = "Company")]
        public string  CompanyCode { get; set; }
    }
}
