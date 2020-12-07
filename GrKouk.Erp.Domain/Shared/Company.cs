using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.CashFlow;

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
        
        private ICollection<TransactorCompanyMapping> _transactorCompanyMappings;
        public ICollection<TransactorCompanyMapping> TransactorCompanyMappings
        {
            get => _transactorCompanyMappings ?? (_transactorCompanyMappings = new List<TransactorCompanyMapping>());
            set => _transactorCompanyMappings = value;
        }

        private ICollection<CompanyWarehouseItemMapping> _warehouseItemCompanyMappings;

        public ICollection<CompanyWarehouseItemMapping> WarehouseItemCompanyMappings
        {
            get => _warehouseItemCompanyMappings ?? (_warehouseItemCompanyMappings = new List<CompanyWarehouseItemMapping>());
            set => _warehouseItemCompanyMappings = value;
        }

        private ICollection<CashFlowAccountCompanyMapping> _cashFlowAccountCompanyMappings;

        public ICollection<CashFlowAccountCompanyMapping> CashFlowAccountCompanyMappings
        {
            get => _cashFlowAccountCompanyMappings ?? (_cashFlowAccountCompanyMappings = new List<CashFlowAccountCompanyMapping>());
            set => _cashFlowAccountCompanyMappings = value;
        }
    }
}