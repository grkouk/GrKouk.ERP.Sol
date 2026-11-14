using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class DiaryMappingsTests
{
    [Fact]
    public void DiaryDto_ToEntity_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<DiaryDto, DiaryDef>(d => d.ToEntity());
    }

    [Fact]
    public void DiaryDto_ToEntity_maps_explicit_values_and_leaves_Id_for_the_database()
    {
        var dto = new DiaryDto
        {
            Name = "Buys diary",
            DiaryType = DiaryTypeEnum.DiaryTypeEnumBuys,
            SelectedDocTypes = "1,2",
            SelectedMatNatures = "3",
            SelectedTransTypes = "4,5"
        };

        var entity = dto.ToEntity();

        Assert.Equal(0, entity.Id);
        Assert.Equal("Buys diary", entity.Name);
        Assert.Equal(DiaryTypeEnum.DiaryTypeEnumBuys, entity.DiaryType);
        Assert.Equal("1,2", entity.SelectedDocTypes);
        Assert.Equal("3", entity.SelectedMatNatures);
        Assert.Equal("4,5", entity.SelectedTransTypes);
    }

    [Fact]
    public void MappingAssert_detects_a_forgotten_property()
    {
        // Sanity check of the helper itself: a mapping that skips a field must fail.
        Func<DiaryDto, DiaryDef> incomplete = d => new DiaryDef
        {
            Name = d.Name,
            DiaryType = d.DiaryType,
            SelectedDocTypes = d.SelectedDocTypes,
            SelectedMatNatures = d.SelectedMatNatures
            // SelectedTransTypes deliberately missing
        };

        var ex = Assert.ThrowsAny<Exception>(
            () => MappingAssert.AllMatchingPropertiesCopied(incomplete));
        Assert.Contains("SelectedTransTypes", ex.Message);
    }
}
