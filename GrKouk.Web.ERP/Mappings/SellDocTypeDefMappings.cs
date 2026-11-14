using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.DocTypeDefs;

namespace GrKouk.Web.ERP.Mappings
{
    public static class SellDocTypeDefMappings
    {
        /// <summary>EF-translatable list projection (the transactor/warehouse defs are optional).</summary>
        public static readonly Expression<Func<SellDocTypeDef, SellDocTypeDefListDto>> ToListDto =
            src => new SellDocTypeDefListDto
            {
                Id = src.Id,
                Code = src.Code,
                Abbr = src.Abbr,
                Name = src.Name,
                Active = src.Active,
                TransTransactorDefId = src.TransTransactorDefId,
                TransTransactorDefName = src.TransTransactorDef.Name,
                TransWarehouseDefId = src.TransWarehouseDefId,
                TransWarehouseDefName = src.TransWarehouseDef.Name,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code
            };
    }
}
