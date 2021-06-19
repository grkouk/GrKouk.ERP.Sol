using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class TransWarehouseDef
    {
        public int Id { get; set; }

        [MaxLength(15)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")] public bool Active { get; set; }

        [Display(Name = "Material Inv.Volume")]
        public InventoryActionEnum MaterialInventoryAction { get; set; }
       
        [Display(Name = "Material Inv.Value")]
        public InventoryValueActionEnum MaterialInventoryValueAction { get; set; }
        [Display(Name = "Material Invoiced Volume")]
        public InventoryActionEnum MaterialInvoicedVolumeAction { get; set; }
        [Display(Name = "Material Invoiced Value")]
        public InventoryValueActionEnum MaterialInvoicedValueAction { get; set; }
        [Display(Name = "Service Inv.Volume")]
        public InventoryActionEnum ServiceInventoryAction { get; set; }
        [Display(Name = "Service Inv.Value")]
        public InventoryValueActionEnum ServiceInventoryValueAction { get; set; }
        [Display(Name = "Expense Inv.Volume")]
        public InventoryActionEnum ExpenseInventoryAction { get; set; }
        [Display(Name = "Expense Inv.Value")]
        public InventoryValueActionEnum ExpenseInventoryValueAction { get; set; }
        [Display(Name = "Income Inv.Volume")]
        public InventoryActionEnum IncomeInventoryAction { get; set; }
        [Display(Name = "Income Inv.Value")]
        public InventoryValueActionEnum IncomeInventoryValueAction { get; set; }

        [Display(Name = "Fixed As.Inv.Volume")]
        public InventoryActionEnum FixedAssetInventoryAction { get; set; }
        [Display(Name = "Fixed As.Inv.Value")]
        public InventoryValueActionEnum FixedAssetInventoryValueAction { get; set; }

        [Display(Name = "Raw Material Inv.Volume")]
        public InventoryActionEnum RawMaterialInventoryAction { get; set; }
        [Display(Name = "Raw Material Inv.Value")]
        public InventoryValueActionEnum RawMaterialInventoryValueAction { get; set; }

        [Display(Name = "Sales Volume")]
        public InventoryActionEnum SalesVolumeAction { get; set; }
        [Display(Name = "Sales Value")]
        public InventoryValueActionEnum SalesValueAction { get; set; }

        [Display(Name = "Buy Volume")]
        public InventoryActionEnum BuyVolumeAction { get; set; }
        [Display(Name = "Buy Value")]
        public InventoryValueActionEnum BuyValueAction { get; set; }

        [Display(Name = "Production Volume")]
        public InventoryActionEnum ProductionVolumeAction { get; set; }
        [Display(Name = "Production Value")]
        public InventoryValueActionEnum ProductionValueAction { get; set; }

        [Display(Name = "Consumption Volume")]
        public InventoryActionEnum ConsumptionVolumeAction { get; set; }
        [Display(Name = "Consumption Value")]
        public InventoryValueActionEnum ConsumptionValueAction { get; set; }

        [Display(Name = "Default Series")]
        public int DefaultDocSeriesId { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}