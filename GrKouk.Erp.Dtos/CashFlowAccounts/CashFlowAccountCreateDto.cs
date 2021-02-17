using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CashFlowAccountCreateDto
    {
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
       
        public string SelectedCompanies { get; set; }
    }
}
