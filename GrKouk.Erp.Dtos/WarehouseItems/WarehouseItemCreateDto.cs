using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class WarehouseItemCreateDto
    {
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Βασική ΜΜ")]
        public int? MainMeasureUnitId { get; set; }
       

        [Display(Name = "Δευ/σα ΜΜ")]
        public int? SecondaryMeasureUnitId { get; set; }

        [Display(Name = "Συν.Mετ.",Prompt = "Συντελεστής μετατροπής σε βασική") ]
        public double SecondaryUnitToMainRate { get; set; }

        [Display(Name = "ΜΜ Αγορών")]
        public int? BuyMeasureUnitId { get; set; }

        [Display(Name = "Συν.Mετ.", Prompt = "Συντελεστής μετατροπής σε βασική")]

        public double BuyUnitToMainRate { get; set; }

        [Display(Name = "ΦΠΑ")]
        public int FpaDefId { get; set; }

        [MaxLength(50)]
        public string BarCode { get; set; }
        [MaxLength(50)]
        public string ManufacturerCode { get; set; }

        [Display(Name = "Κατ.Υλικού")]
        public int MaterialCategoryId { get; set; }
        /// <summary>
        /// Τύπος υλικού, Σετ, Κανονικό, Συντιθέμενο
        /// </summary>
        [Display(Name = "Τύπος Υλικού", Prompt = "Τύπος υλικού, Σετ, Κανονικό, Συντιθέμενο")]

        public MaterialTypeEnum MaterialType { get; set; }
        [Display(Name = "Φύση Είδους", Prompt = "Υλικό,Υπηρεσία,Παγιο,Δαπάνη")]
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
        [Display(Name = "Καθ.Τιμή")]
        public decimal PriceNetto { get; set; }
        [Display(Name = "Μικτή Τιμή")]
        public decimal PriceBrutto { get; set; }
        public int CompanyId { get; set; }
        public int CashRegCategoryId { get; set; }

    }
}
