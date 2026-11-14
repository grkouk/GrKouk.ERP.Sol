using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;

namespace GrKouk.Web.ERP.Mappings
{
    public static class WrItemCodeMappings
    {
        /// <summary>
        /// EF-translatable projection. CompanyName/CompanyCode, TransactorName and the *Name
        /// enum descriptions are filled in after paging by the caller.
        /// </summary>
        public static readonly Expression<Func<WrItemCode, WrItemCodeListDto>> ToListDto = src => new WrItemCodeListDto
        {
            Id = src.Id,
            CompanyId = src.CompanyId,
            CodeType = src.CodeType,
            Code = src.Code,
            TransactorId = src.TransactorId,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItem = src.WarehouseItem,
            WarehouseItemName = src.WarehouseItem.Name,
            CodeUsedUnit = src.CodeUsedUnit,
            RateToMainUnit = src.RateToMainUnit,
            BuyCodeUsedUnit = src.BuyCodeUsedUnit,
            BuyRateToMainUnit = src.BuyRateToMainUnit,
            SellCodeUsedUnit = src.SellCodeUsedUnit,
            SellRateToMainUnit = src.SellRateToMainUnit
        };
    }
}
