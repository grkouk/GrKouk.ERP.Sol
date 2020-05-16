using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class WrItemCodeModifyDto
    {
        public int Id { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        [Display(Name = "Code Type")]
        public WarehouseItemCodeTypeEnum CodeType { get; set; }
        [MaxLength(30)]
        public string Code { get; set; }
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        [Display(Name = "Warehouse Item")]
        public int WarehouseItemId { get; set; }

        public WarehouseItemCodeUsedUnitEnum CodeUsedUnit { get; set; }
        public double RateToMainUnit { get; set; }
        [Display(Name = "Buy Unit")]
        public WarehouseItemCodeUsedUnitEnum BuyCodeUsedUnit { get; set; }
        [Display(Name = "Buy Rate to Main")]
        public double BuyRateToMainUnit { get; set; }
        [Display(Name = "Sell Unit")]
        public WarehouseItemCodeUsedUnitEnum SellCodeUsedUnit { get; set; }
        [Display(Name = "Buy Rate to Main")]
        public double SellRateToMainUnit { get; set; }
    }
}
