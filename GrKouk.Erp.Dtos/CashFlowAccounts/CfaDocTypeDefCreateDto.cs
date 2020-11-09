using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CfaDocTypeDefCreateDto
    {
       
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
       
        [Display(Name = "Κίν.Χρημ.Λογ.")]
        public int CashFlowTransactionDefId { get; set; }
       

        public int CompanyId { get; set; }
       
        [Display(Name = "Default Section")]
        public int SectionId { get; set; }
    }
}