using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Dtos.CashFlowAccounts;

namespace GrKouk.Web.ERP.Mappings
{
    /// <summary>EF-translatable list projections for the cash flow definitions (transaction, doc type, doc series).</summary>
    public static class CashFlowDefinitionMappings
    {
        public static readonly Expression<Func<CashFlowTransactionDef, CFATransactionDefListDto>> TransactionDefToListDto =
            src => new CFATransactionDefListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                CompanyName = src.Company.Name,
                CompanyCode = src.Company.Code
            };

        public static readonly Expression<Func<CashFlowDocTypeDef, CfaDocTypeDefListDto>> DocTypeDefToListDto =
            src => new CfaDocTypeDefListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                Active = src.Active,
                // AutoMapper left this 0 (no source member named CfaTransactionDefId); now the real FK.
                CfaTransactionDefId = src.CashFlowTransactionDefId,
                CfaTransactionDefName = src.CashFlowTransactionDefinition.Name,
                CfaTransactionDefCode = src.CashFlowTransactionDefinition.Code,
                CompanyId = src.CompanyId,
                CompanyName = src.Company.Name,
                CompanyCode = src.Company.Code,
                SectionId = src.SectionId
            };

        public static readonly Expression<Func<CashFlowDocSeriesDef, CfaDocSeriesDefListDto>> DocSeriesDefToListDto =
            src => new CfaDocSeriesDefListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                Active = src.Active,
                // AutoMapper left this 0 (no source member named CfaDocTypeDefId); now the real FK.
                CfaDocTypeDefId = src.CashFlowDocTypeDefId,
                CfaDocTypeDefName = src.CashFlowDocTypeDefinition.Name,
                CfaDocTypeDefCode = src.CashFlowDocTypeDefinition.Code,
                CompanyId = src.CompanyId,
                CompanyName = src.Company.Name,
                CompanyCode = src.Company.Code
            };
    }
}
