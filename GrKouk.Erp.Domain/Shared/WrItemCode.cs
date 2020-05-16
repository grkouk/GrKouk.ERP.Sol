using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class WrItemCode
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public WarehouseItemCodeTypeEnum CodeType { get; set; }
        [MaxLength(30)]
        public string Code { get; set; }
        public int TransactorId { get; set; }
        public int WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
        public WarehouseItemCodeUsedUnitEnum CodeUsedUnit { get; set; }
        public double RateToMainUnit { get; set; }
        public WarehouseItemCodeUsedUnitEnum BuyCodeUsedUnit { get; set; }
        public double BuyRateToMainUnit { get; set; }
        public WarehouseItemCodeUsedUnitEnum SellCodeUsedUnit { get; set; }
        public double SellRateToMainUnit { get; set; }

    }
}