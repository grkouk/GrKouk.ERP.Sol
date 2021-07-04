using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class WarehouseItemListDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }


        public string Name { get; set; }


        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Βασική ΜΜ")]
        public int? MainMeasureUnitId { get; set; }
        public string MainMeasureUnitCode { get; set; }

        [Display(Name = "ΜΜ Αγορών")]
        public int? BuyMeasureUnitId { get; set; }
        public string BuyMeasureUnitCode { get; set; }

        [Display(Name = "ΦΠΑ")]
        public int FpaDefId { get; set; }
        [Display(Name = "ΦΠΑ")]
        public string FpaDefName { get; set; }

        [MaxLength(50)]
        public string BarCode { get; set; }
        [MaxLength(50)]
        public string ManufacturerCode { get; set; }

        public int MaterialCategoryId { get; set; }
        [Display(Name = "Κατ.Είδους")]
        public string MaterialCategoryName { get; set; }


        public MaterialTypeEnum MaterialType { get; set; }
        [Display(Name = "Τύπος Είδους")]
        public string MaterialTypeName
        {
            get
            {
                string ret;
                switch (MaterialType)
                {

                    case MaterialTypeEnum.MaterialTypeNormal:
                        ret = "Κανονικό";
                        break;
                    case MaterialTypeEnum.MaterialTypeSet:
                        ret = "Σετ";
                        break;
                    case MaterialTypeEnum.MaterialTypeComposed:
                        ret = "Συντιθέμενο";
                        break;
                    default:
                        ret = "Απροσδιόριστο";
                       break;
                }
                return ret;
            }
            
        }
        [Display(Name = "Καθ.Τιμή")]
        public decimal PriceNetto { get; set; }
        [Display(Name = "Μικτή Τιμή")]
        public decimal PriceBrutto { get; set; }
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
        [Display(Name = "Φύση Είδους", Prompt = "Υλικό,Υπηρεσία,Παγιο,Δαπάνη")]
        public string WarehouseItemNatureName {
            get
            {
                string ret;
                switch (WarehouseItemNature)
                {
                    case WarehouseItemNatureEnum.WarehouseItemNatureUndefined:
                        ret = "Απροσδιόριστο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureMaterial:
                        ret = "Υλικό";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureService:
                        ret = "Υπηρεσία";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureExpense:
                        ret = "Δαπάνη";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset:
                        ret = "Πάγιο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureIncome:
                        ret = "Εσοδο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureRawMaterial:
                        ret = "Πρώτη Υλη";
                        break;
                    default:
                        ret = "Απροσδιόριστο";
                        break;
                }
                return ret;
            }
        }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }

    public class WarehouseItemBigClass
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }


        public string Name { get; set; }


        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        //[Display(Name = "Βασική ΜΜ")]
        //public int? MainMeasureUnitId { get; set; }
        //public string MainMeasureUnitCode { get; set; }

        //[Display(Name = "ΜΜ Αγορών")]
        //public int? BuyMeasureUnitId { get; set; }
        //public string BuyMeasureUnitCode { get; set; }

        [Display(Name = "ΦΠΑ")]
        public int FpaDefId { get; set; }
        [Display(Name = "ΦΠΑ")]
        public string FpaDefName { get; set; }

        //[MaxLength(50)]
        //public string BarCode { get; set; }
        //[MaxLength(50)]
        //public string ManufacturerCode { get; set; }

        public int MaterialCategoryId { get; set; }
        [Display(Name = "Κατ.Είδους")]
        public string MaterialCategoryName { get; set; }


        public MaterialTypeEnum MaterialType { get; set; }
        [Display(Name = "Τύπος Είδους")]
        public string MaterialTypeName
        {
            get
            {
                string ret;
                switch (MaterialType)
                {

                    case MaterialTypeEnum.MaterialTypeNormal:
                        ret = "Κανονικό";
                        break;
                    case MaterialTypeEnum.MaterialTypeSet:
                        ret = "Σετ";
                        break;
                    case MaterialTypeEnum.MaterialTypeComposed:
                        ret = "Συντιθέμενο";
                        break;
                    default:
                        ret = "Απροσδιόριστο";
                        break;
                }
                return ret;
            }

        }
        [Display(Name = "Καθ.Τιμή")]
        public decimal PriceNetto { get; set; }
        [Display(Name = "Μικτή Τιμή")]
        public decimal PriceBrutto { get; set; }
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
        [Display(Name = "Φύση Είδους", Prompt = "Υλικό,Υπηρεσία,Παγιο,Δαπάνη")]
        public string WarehouseItemNatureName
        {
            get
            {
                string ret;
                switch (WarehouseItemNature)
                {
                    case WarehouseItemNatureEnum.WarehouseItemNatureUndefined:
                        ret = "Απροσδιόριστο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureMaterial:
                        ret = "Υλικό";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureService:
                        ret = "Υπηρεσία";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureExpense:
                        ret = "Δαπάνη";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset:
                        ret = "Πάγιο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureIncome:
                        ret = "Εσοδο";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureRawMaterial:
                        ret = "Πρώτη Υλη";
                        break;
                    default:
                        ret = "Απροσδιόριστο";
                        break;
                }
                return ret;
            }
        }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }
}
