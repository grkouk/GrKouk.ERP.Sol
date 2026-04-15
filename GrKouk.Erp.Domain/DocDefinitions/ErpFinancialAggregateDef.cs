using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    /// <summary>
    /// Ορισμός αθροιστικού οικονομικού είδους. Αντιστοιχίζει το ζεύγος
    /// (ItemNature, FpaDef) σε ένα συγκεκριμένο WarehouseItem που
    /// χρησιμοποιείται ως αθροιστικό για BuyDoc και ItemSyncV2.
    /// Μόνο για Material/Service/FixedAsset — Expense/Income μένουν 1:1.
    /// </summary>
    public class ErpFinancialAggregateDef
    {
        public int Id { get; set; }

        [Display(Name = "Φύση Είδους")]
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }

        [Display(Name = "ΦΠΑ")]
        public int FpaDefId { get; set; }
        public virtual FpaDef FpaDef { get; set; }

        [Display(Name = "Αθροιστικό Είδος")]
        public int WarehouseItemId { get; set; }
        public virtual WarehouseItem WarehouseItem { get; set; }

        [Display(Name = "Ενεργό")]
        public bool Active { get; set; } = true;

        [MaxLength(500)]
        [Display(Name = "Σημειώσεις")]
        public string Notes { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
