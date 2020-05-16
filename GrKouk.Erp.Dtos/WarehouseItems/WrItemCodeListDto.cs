using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
   public  class WrItemCodeListDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public WarehouseItemCodeTypeEnum CodeType { get; set; }
        public string CodeTypeName { get; set; }
        public string Code { get; set; }
        public int TransactorId { get; set; }
        public string TransactorName { get; set; }
        public int WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
        public string WarehouseItemName { get; set; }
        public WarehouseItemCodeUsedUnitEnum CodeUsedUnit { get; set; }
        public string CodeUsedUnitName { get; set; }
        public double RateToMainUnit { get; set; }
        public WarehouseItemCodeUsedUnitEnum BuyCodeUsedUnit { get; set; }
        public string BuyCodeUsedUnitName { get; set; }
        public double BuyRateToMainUnit { get; set; }
        public WarehouseItemCodeUsedUnitEnum SellCodeUsedUnit { get; set; }
        public string SellCodeUsedUnitName { get; set; }
        public double SellRateToMainUnit { get; set; }
    }
}
