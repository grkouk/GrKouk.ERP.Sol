using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.MainEntities.Warehouse
{
    public class CompanyWarehouseItemMapping
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
    }
}