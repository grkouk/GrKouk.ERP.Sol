using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class WarehouseItemSearchListDto
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }
       
        public string WarehouseItemNatureName
        {
            get
            {
               
                string ret = "";
                switch (WarehouseItemNature)
                {
                    case WarehouseItemNatureEnum.WarehouseItemNatureUndefined:
                        ret = "{{Απρ}}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureMaterial:
                        ret = "{Υλ}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureService:
                        ret = "{Υπ}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureExpense:
                        ret = "{Δαπ}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset:
                        ret = "{Πάγ}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureIncome:
                        ret = "{Εσο}";
                        break;
                    case WarehouseItemNatureEnum.WarehouseItemNatureRawMaterial:
                        ret = "{ΠρΥλ}";
                        break;
                    default:
                        ret = "{{Απρ}}";
                        break;
                }
                return ret;
            }
        }

        public string Value => this.Id.ToString();

        public string Label
        {
            get
            {
               var r= $"{this.Name} {{{this.Code}}} {this.WarehouseItemNatureName}";
               return r;
            }
        }
    }
}