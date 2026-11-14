using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashRegister;

namespace GrKouk.Web.ERP.Mappings
{
    public static class CrCatWarehouseItemMappings
    {
        public static readonly Expression<Func<CrCatWarehouseItem, CashRegCatProductListDto>> ToListDto =
            src => new CashRegCatProductListDto
            {
                Id = src.Id,
                ClientProfileName = src.ClientProfile.Name,
                CompanyCode = src.ClientProfile.Company.Code,
                CashRegCategoryName = src.CashRegCategory.Name,
                WarehouseItemName = src.WarehouseItem.Name
            };
    }
}
