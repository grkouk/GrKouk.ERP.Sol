using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class Company
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Base Currency")]
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }

        public ICollection<TransactorCompanyMapping> TransactorCompanyMappings { get; set; }
    }
}