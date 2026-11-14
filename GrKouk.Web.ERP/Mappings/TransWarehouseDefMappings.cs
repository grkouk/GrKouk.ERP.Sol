using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.WarehouseTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class TransWarehouseDefMappings
    {
        public static readonly Expression<Func<TransWarehouseDef, TransWarehouseDefListDto>> ToListDto =
            src => new TransWarehouseDefListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                Active = src.Active,
                CompanyName = src.Company.Name
            };
    }
}
