using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.Transactors
{
    public class ProductBigClass
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string MaterialCategoryName { get; set; }
        public bool Active { get; set; }
        public MaterialTypeEnum MaterialType { get; set; }
        public WarehouseItemNatureEnum WarehouseItemNature { get; set; }

        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }
}