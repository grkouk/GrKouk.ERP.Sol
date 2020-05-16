using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    /// <summary>
    /// Τύπος παραστατικού αγορών
    /// </summary>
    public class BuyDocTypeDef
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [Display(Name = "Συντόμευση", Prompt = "Συντόμευση του τύπου")]
        [MaxLength(20)]
        public string Abbr { get; set; }


        [MaxLength(200)]
        public string Name { get; set; }

        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        //[Display(Name = "Κίνηση Προμηθευτή")]
        //public int? TransSupplierDefId { get; set; }
        //public TransSupplierDef TransSupplierDef { get; set; }
        [Display(Name = "Κίνηση Συναλ/νου")]
        public int? TransTransactorDefId { get; set; }
        public TransTransactorDef TransTransactorDef { get; set; }

        [Display(Name = "Κίνηση Αποθήκης")]
        public int? TransWarehouseDefId { get; set; }
        public TransWarehouseDef TransWarehouseDef { get; set; }
        public PriceTypeEnum UsedPrice { get; set; }
        [MaxLength(200)]
        public string SelectedWarehouseItemNatures { get; set; }
        [MaxLength(200)]
        public string AllowedTransactorTypes { get; set; }
        [MaxLength(200)]
        public string AllowedSectionTypes { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        [Display(Name = "Default Section")]
        public int SectionId { get; set; }
    }
}
