using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class ProductSelectorListDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }


        public string Name { get; set; }


        public bool Active { get; set; }

        public int MaterialCategoryId { get; set; }

        public string MaterialCateroryName { get; set; }


        public MaterialTypeEnum MaterialType { get; set; }

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
       
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
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
}