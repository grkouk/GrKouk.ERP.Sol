﻿using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CashFlowAccountCreateDto
    {
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
        public string SelectedCompanies { get; set; }
    }
}
