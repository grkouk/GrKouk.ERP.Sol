using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Dtos.WarehouseItems
{
    public class WrItemCodeCreateDto
    {
       

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
        [Display(Name = "Sell Rate to Main")]
        public double SellRateToMainUnit { get; set; }

        public static WrItemCodeCreateDto Map(WrItemCode entityToMap)
        {
            return new WrItemCodeCreateDto()
            {
                CompanyId = entityToMap.CompanyId,
                CodeType = entityToMap.CodeType,
                Code = entityToMap.Code,
                TransactorId = entityToMap.TransactorId,
                WarehouseItemId = entityToMap.WarehouseItemId,
                CodeUsedUnit = entityToMap.CodeUsedUnit,
                RateToMainUnit = entityToMap.RateToMainUnit,
                BuyCodeUsedUnit = entityToMap.BuyCodeUsedUnit,
                BuyRateToMainUnit = entityToMap.BuyRateToMainUnit,
                SellCodeUsedUnit = entityToMap.SellCodeUsedUnit,
                SellRateToMainUnit = entityToMap.SellRateToMainUnit
            };
        }

        public WrItemCode MapToEntity()
        {
            return new WrItemCode()
            {
                CompanyId = CompanyId,
                CodeType = CodeType,
                Code = Code,
                TransactorId = TransactorId,
                WarehouseItemId = WarehouseItemId,
                CodeUsedUnit = CodeUsedUnit,
                RateToMainUnit = RateToMainUnit,
                BuyCodeUsedUnit = BuyCodeUsedUnit,
                BuyRateToMainUnit = BuyRateToMainUnit,
                SellCodeUsedUnit = SellCodeUsedUnit,
                SellRateToMainUnit = SellRateToMainUnit
            };
        }
    }
}
