using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class WarehouseItem
    {
        //private ICollection<WarehouseItemCode> _warehouseItemCodes;
        private ICollection<WrItemCode> _warehouseItemCodes;
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Βασική Μονάδα Μέτρησης")]
        public int MainMeasureUnitId { get; set; }
        public virtual MeasureUnit MainMeasureUnit { get; set; }

        [Display(Name = "Δευτερεύουσα Μονάδα Μέτρησης")]
        public int SecondaryMeasureUnitId { get; set; }
        public virtual MeasureUnit SecondaryMeasureUnit { get; set; }
        [Display(Name = "Συντελεστής μετατροπής σε βασική")]
        public double SecondaryUnitToMainRate { get; set; }

        [Display(Name = "Μονάδα Μέτρησης Αγορών")]
        public int BuyMeasureUnitId { get; set; }
        public virtual MeasureUnit BuyMeasureUnit { get; set; }
        [Display(Name = "Συντελεστής μετατροπής σε βασική")]
        public double BuyUnitToMainRate { get; set; }

        [Display(Name = "ΦΠΑ")]
        public int FpaDefId { get; set; }
        public virtual FpaDef FpaDef { get; set; }

        [MaxLength(50)]
        public string BarCode { get; set; }
        [MaxLength(50)]
        public string ManufacturerCode { get; set; }

        /// <summary>
        /// Κατηγορία υλικού
        /// </summary>
        [Display(Name = "Κατηγορία Υλικού")]
        public int MaterialCategoryId { get; set; }
        public MaterialCategory MaterialCaterory { get; set; }


        /// <summary>
        /// Τύπος υλικού, Σετ, Κανονικό, Συντιθέμενο
        /// </summary>
        [Display(Name = "Τύπος Υλικού", Prompt = "Τύπος υλικού, Σετ, Κανονικό, Συντιθέμενο")]

        public MaterialTypeEnum MaterialType { get; set; }
        [Display(Name = "Φύση Είδους", Prompt = "Υλικό,Υπηρεσία,Παγιο,Δαπάνη")]
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public decimal PriceNetto { get; set; }
        public decimal PriceBrutto { get; set; }
        public virtual ICollection<WrItemCode> WarehouseItemCodes
        {
            get => _warehouseItemCodes ??= new List<WrItemCode>();
            set => _warehouseItemCodes = value;
        }

        private ICollection<CompanyWarehouseItemMapping> _companyWarehouseItemMappings;
        public virtual ICollection<CompanyWarehouseItemMapping> CompanyMappings
        {
            get => _companyWarehouseItemMappings ??= new List<CompanyWarehouseItemMapping>();
            set => _companyWarehouseItemMappings = value;
        }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateLastModified { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
