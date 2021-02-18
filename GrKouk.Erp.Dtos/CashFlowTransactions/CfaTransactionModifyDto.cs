using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CashFlowTransactions
{
    public class CfaTransactionModifyDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Doc Series")]
        public int DocSeriesId { get; set; }

        public int DocTypeId { get; set; }
        [Display(Name = "Reference Code")]
        public string TransRefCode { get; set; }
        [Display(Name = "Transactor")]
        public int CashFlowAccountId { get; set; }
        [Display(Name = "Section")]
        public int SectionId { get; set; }
        public int CreatorId { get; set; }
        [Display(Name = "Fiscal Period")]
        public int FiscalPeriodId { get; set; }
        public CashFlowAccountActionsEnum CfaAction { get; set; }
        

        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Amount")]
        public decimal AmountSum { get; set; }

        [MaxLength(500)]
        [Display(Name = "Etiology")]
        public string Etiology { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}