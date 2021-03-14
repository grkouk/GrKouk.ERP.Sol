using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CashFlowTransactions
{
    public class CfaTransactionListDto
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }

        public int DocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string DocSeriesName { get; set; }
        [Display(Name = "Series")]
        public string DocSeriesCode { get; set; }

        public int DocTypeId { get; set; }
        [Display(Name = "Ref")]
        public string TransRefCode { get; set; }
        public int CashFlowAccountId { get; set; }
        [Display(Name = "Account")]
        public string CashFlowAccountName { get; set; }
       

        public int SectionId { get; set; }
        [Display(Name = "Section")]
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }
        public int CreatorSectionId { get; set; }
        public string CreatorSectionCode { get; set; }
        public int FiscalPeriodId { get; set; }
        public CashFlowAccountActionsEnum CfaAction { get; set; }
        

       
        public decimal Amount { get; set; }

        public decimal TransAmount { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Deposit")]
        public decimal DepositAmount =>
            (CfaAction.Equals(CashFlowAccountActionsEnum.CfaActionDeposit) ||
             CfaAction.Equals(CashFlowAccountActionsEnum.CfaActionNegativeDeposit)
                ? TransAmount
                : 0);

        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Withdraw")]
        public decimal WithdrawAmount => (CfaAction.Equals(CashFlowAccountActionsEnum.CfaActionWithdraw) ||
                                          CfaAction.Equals(CashFlowAccountActionsEnum.CfaActionNegativeWithdraw)
            ? TransAmount
            : 0);


       
        public int CompanyId { get; set; }  
        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
    }

   
}