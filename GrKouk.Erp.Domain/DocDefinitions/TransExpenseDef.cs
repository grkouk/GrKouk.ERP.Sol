using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class TransExpenseDef
    {
        public int Id { get; set; }
       
        [MaxLength(20)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
        //public FinActionsEnum FinancialAction { get; set; }
        [Display(Name = "Inventory Action")]
        public InventoryActionEnum InventoryAction { get; set; }
        [Display(Name = "Inventory Value Action")]
        public InventoryValueActionEnum InventoryValueAction { get; set; }
        [Display(Name = "Default Series")]
        public int? DefaultDocSeriesId { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}