using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.TransactorTransactions
{
    public class TransactorTransListDto
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }

        public int TransTransactorDocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string TransTransactorDocSeriesName { get; set; }
        [Display(Name = "Series")]
        public string TransTransactorDocSeriesCode { get; set; }

        public int TransTransactorDocTypeId { get; set; }
        [Display(Name = "Ref")]
        public string TransRefCode { get; set; }
        public int TransactorId { get; set; }
        [Display(Name = "Transactor")]
        public string TransactorName { get; set; }
       

        public int SectionId { get; set; }
        [Display(Name = "Section")]
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }

        public int FiscalPeriodId { get; set; }

        public FinActionsEnum FinancialAction { get; set; }

        public decimal FpaRate { get; set; }
        public decimal DiscountRate { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }

        public decimal TransFpaAmount { get; set; }
        public decimal TransNetAmount { get; set; }
        public decimal TransDiscountAmount { get; set; }
        /// <summary>
        /// Transaction amount without taxes
        /// </summary>
        public decimal TotalNetAmount
        {
            get => TransNetAmount - TransDiscountAmount;

        }
        public decimal TotalAmount
        {
            get => TransNetAmount + TransFpaAmount - TransDiscountAmount;

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Debit")]
        public decimal DebitAmount
        {
            get => (FinancialAction.Equals(FinActionsEnum.FinActionsEnumDebit) ||
                    FinancialAction.Equals(FinActionsEnum.FinActionsEnumNegativeDebit)
                ? TotalAmount
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Credit")]
        public decimal CreditAmount => (FinancialAction.Equals(FinActionsEnum.FinActionsEnumCredit) ||
                                        FinancialAction.Equals(FinActionsEnum.FinActionsEnumNegativeCredit)
            ? TotalAmount
            : 0);


        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Net Debit ")]
        public decimal DebitNetAmount
        {
            get => (FinancialAction.Equals(FinActionsEnum.FinActionsEnumDebit) ||
                    FinancialAction.Equals(FinActionsEnum.FinActionsEnumNegativeDebit)
                ? TotalNetAmount
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Net Credit")]
        public decimal CreditNetAmount => (FinancialAction.Equals(FinActionsEnum.FinActionsEnumCredit) ||
                                        FinancialAction.Equals(FinActionsEnum.FinActionsEnumNegativeCredit)
            ? TotalNetAmount
            : 0);

        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
    }
}