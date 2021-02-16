using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CashFlowAccountModifyDto
    {
        public int Id { get; set; }

        [MaxLength(20)] 
        [Required]
        public string Code { get; set; }

        [MaxLength(200)] 
        [Required]
        public string Name { get; set; }
        
        public string SelectedCompanies { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateLastModified { get; set; }
    }
}