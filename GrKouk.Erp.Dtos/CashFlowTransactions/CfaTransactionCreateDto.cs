using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CashFlowTransactions
{
   public  class CfaTransactionCreateDto
    {
        //public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Doc Series")]
        [Required]
        public int DocSeriesId { get; set; }

        public int DocTypeId { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Display(Name = "Account")]
        [Required]
        public int CashFlowAccountId { get; set; }
        [Display(Name = "Sector")]
        public int SectionId { get; set; }
        public int CreatorId { get; set; }
        [Display(Name = "Fiscal Period")]
        public int FiscalPeriodId { get; set; }
        public CashFlowAccountActionsEnum CfaAction { get; set; }
        
       [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        
        [MaxLength(500)]
        public string Etiology { get; set; }
        [Required]
        public int CompanyId { get; set; }

        //[Timestamp]
        //public byte[] Timestamp { get; set; }
    }
}
