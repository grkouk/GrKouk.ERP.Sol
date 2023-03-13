using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.CashFlow
{
    public class CashFlowAccount
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateLastModified { get; set; }

        private ICollection<CashFlowAccountCompanyMapping> _companyMappings;
        public ICollection<CashFlowAccountCompanyMapping> CompanyMappings
        {
            get => _companyMappings ??= new List<CashFlowAccountCompanyMapping>();
            set => _companyMappings = value;
        }
    }
}
