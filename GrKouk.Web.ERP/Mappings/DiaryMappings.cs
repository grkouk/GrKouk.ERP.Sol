using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;

namespace GrKouk.Web.ERP.Mappings
{
    public static class DiaryMappings
    {
        public static DiaryDef ToEntity(this DiaryDto dto) => new()
        {
            Name = dto.Name,
            DiaryType = dto.DiaryType,
            SelectedDocTypes = dto.SelectedDocTypes,
            SelectedMatNatures = dto.SelectedMatNatures,
            SelectedTransTypes = dto.SelectedTransTypes
        };
    }
}
