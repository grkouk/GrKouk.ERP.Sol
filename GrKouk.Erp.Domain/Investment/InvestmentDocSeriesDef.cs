﻿using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.Investment
{
    public class InvestmentDocSeriesDef
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Τύπος Παραστατικού")]
        public int CashFlowDocTypeDefId { get; set; }
        public InvestmentDocTypeDef InvestmentDocTypeDef { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}