using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CfaDocTypeDefListDto
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
       
        [Display(Name = "Κίν.Χρημ.Λογ.")]
        public int CfaTransactionDefId { get; set; }
        public string CfaTransactionDefName { get; set; }
        public string CfaTransactionDefCode { get; set; }

        public int CompanyId { get; set; }
        public string  CompanyName { get; set; }
        public string  CompanyCode { get; set; }

        [Display(Name = "Default Section")]
        public int SectionId { get; set; }
    }
}