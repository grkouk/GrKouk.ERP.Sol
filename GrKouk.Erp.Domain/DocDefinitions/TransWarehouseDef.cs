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

        [Display(Name = "Material Inventory")]
        public InventoryActionEnum MaterialInventoryAction { get; set; }
       
        [Display(Name = "Material InvValue")]
        public InventoryValueActionEnum MaterialInventoryValueAction { get; set; }
        [Display(Name = "Material Invoiced Volume")]
        public InventoryActionEnum MaterialInvoicedVolumeAction { get; set; }
        [Display(Name = "Material Invoiced Value")]
        public InventoryValueActionEnum MaterialInvoicedValueAction { get; set; }
        [Display(Name = "Service InvAct")]
        public InventoryActionEnum ServiceInventoryAction { get; set; }
        [Display(Name = "Service InvValAct")]
        public InventoryValueActionEnum ServiceInventoryValueAction { get; set; }
        [Display(Name = "Expense InvAct")]
        public InventoryActionEnum ExpenseInventoryAction { get; set; }
        [Display(Name = "Expense InvValAct")]
        public InventoryValueActionEnum ExpenseInventoryValueAction { get; set; }
        [Display(Name = "Income InvAct")]
        public InventoryActionEnum IncomeInventoryAction { get; set; }
        [Display(Name = "Income InvValAct")]
        public InventoryValueActionEnum IncomeInventoryValueAction { get; set; }

        [Display(Name = "Fixed As.InvAct")]
        public InventoryActionEnum FixedAssetInventoryAction { get; set; }
        [Display(Name = "Fixed As.InvValAct")]
        public InventoryValueActionEnum FixedAssetInventoryValueAction { get; set; }

        [Display(Name = "Raw Material InvAct")]
        public InventoryActionEnum RawMaterialInventoryAction { get; set; }
        [Display(Name = "RawMaterial InvValAct")]
        public InventoryValueActionEnum RawMaterialInventoryValueAction { get; set; }

        [Display(Name = "Default Series")]
        public int DefaultDocSeriesId { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}