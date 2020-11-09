using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CfaDocSeriesDefListDto
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Τύπος Παραστατικού")]
        public int CfaDocTypeDefId { get; set; }
        public string CfaDocTypeDefName { get; set; }
        public string CfaDocTypeDefCode { get; set; }

        public int CompanyId { get; set; }
        public string  CompanyName { get; set; }
        public string  CompanyCode { get; set; }
    }
}