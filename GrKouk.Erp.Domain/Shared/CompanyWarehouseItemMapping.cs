namespace GrKouk.Erp.Domain.Shared
{
    public class CompanyWarehouseItemMapping
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
    }
}