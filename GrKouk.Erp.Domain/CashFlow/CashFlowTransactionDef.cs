using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.CashFlow
{
    public class CashFlowTransactionDef
    {
        public int Id { get; set; }
       
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
        public virtual Company Company { get; set; }
    }
}
