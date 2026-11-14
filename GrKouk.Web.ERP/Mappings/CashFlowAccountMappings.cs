using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Dtos.CashFlowAccounts;

namespace GrKouk.Web.ERP.Mappings
{
    public static class CashFlowAccountMappings
    {
        /// <summary>New entity from the Create form. Company mappings and dates are set by the caller.</summary>
        public static CashFlowAccount ToEntity(this CashFlowAccountCreateDto dto) => new()
        {
            Code = dto.Code,
            Name = dto.Name
        };

        /// <summary>Edit form model. SelectedCompanies is set by the caller.</summary>
        public static CashFlowAccountModifyDto ToModifyDto(this CashFlowAccount src) => new()
        {
            Id = src.Id,
            Code = src.Code,
            Name = src.Name,
            DateCreated = src.DateCreated,
            DateLastModified = src.DateLastModified
        };

        /// <summary>
        /// Copies the editable fields onto a loaded entity. Id, the dates and the company
        /// mappings are deliberately not touched (the Edit form does not post the dates).
        /// </summary>
        public static void ApplyTo(this CashFlowAccountModifyDto dto, CashFlowAccount entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
        }
    }
}
