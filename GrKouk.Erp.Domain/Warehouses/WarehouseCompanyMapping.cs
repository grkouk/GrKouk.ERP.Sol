using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.Warehouses
{
    public class WarehouseCompanyMapping
    {
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
