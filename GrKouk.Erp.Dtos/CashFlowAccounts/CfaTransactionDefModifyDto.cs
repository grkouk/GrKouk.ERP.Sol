using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CfaTransactionDefModifyDto
    {
        public int Id { get; set; }
       
        [MaxLength(20)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
        
        public CashFlowAccountActionsEnum CfaAction { get; set; }
       
        [Display(Name = "Default Series")]
        public int DefaultDocSeriesId { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }
      
    }
}