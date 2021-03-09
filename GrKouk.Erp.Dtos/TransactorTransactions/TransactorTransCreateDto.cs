using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.TransactorTransactions
{
   public  class TransactorTransCreateDto
    {
        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Doc Series")]
        [Required]
        public int TransTransactorDocSeriesId { get; set; }

        public int TransTransactorDocTypeId { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Display(Name = "Transactor")]
        [Required]
        public int TransactorId { get; set; }
        [Display(Name = "Sector")]
        public int SectionId { get; set; }
        public int CreatorId { get; set; }
        [Display(Name = "Fiscal Period")]
        public int FiscalPeriodId { get; set; }

        public FinActionsEnum FinancialAction { get; set; }
        [Display(Name = "VAT Rate")]
        public decimal FpaRate { get; set; }
        [Display(Name = "Discount Rate")]
        public decimal DiscountRate { get; set; }
        [Display(Name = "VAT Amount")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Sum Amount")]
        public decimal AmountSum => (AmountNet + AmountFpa - AmountDiscount);
       

        [MaxLength(500)]
        public string Etiology { get; set; }
        [Required]
        public int CompanyId { get; set; }

        [Display(Name = "Cash Flow Account")]
        public int CfAccountId { get; set; }
       
    }
}
