﻿using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Dtos.CashFlowAccounts
{
    public class CfaTransactionDefCreateDto
    {
       
       
        [MaxLength(20)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
        public FinActionsEnum FinancialTransAction { get; set; }
       
       
        [Display(Name = "Default Series")]
        public int DefaultDocSeriesId { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }
      
    }
}