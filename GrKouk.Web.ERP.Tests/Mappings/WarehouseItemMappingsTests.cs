using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class WarehouseItemMappingsTests
{
    [Fact]
    public void Entity_ToCreateDto_copies_every_matching_property()
    {
        var dto = MappingAssert.AllMatchingPropertiesCopied<WarehouseItem, WarehouseItemCreateDto>(
            s => s.ToCreateDto(), nameof(WarehouseItemCreateDto.CashRegCategoryId));

        Assert.Null(dto.SelectedCompanies);
    }

    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<WarehouseItemCreateDto, WarehouseItem>(
            d => d.ToEntity(), nameof(WarehouseItemCreateDto.CashRegCategoryId));

        Assert.Equal(0, entity.Id);
        Assert.Empty(entity.CompanyMappings);
    }

    [Fact]
    public void CreateDto_ToEntity_maps_missing_units_to_zero()
    {
        var entity = new WarehouseItemCreateDto().ToEntity();

        Assert.Equal(0, entity.MainMeasureUnitId);
        Assert.Equal(0, entity.SecondaryMeasureUnitId);
        Assert.Equal(0, entity.BuyMeasureUnitId);
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property_and_the_timestamp()
    {
        var src = MappingAssert.CreateFilled<WarehouseItem>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToModifyDto(),
            nameof(WarehouseItemModifyDto.CashRegCategoryId));

        Assert.Equal(src.Timestamp, dto.Timestamp);
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_editable_fields()
    {
        MappingAssert.AllMatchingPropertiesCopied<WarehouseItemModifyDto, WarehouseItem>(
            d => { var e = new WarehouseItem(); d.ApplyTo(e); return e; },
            nameof(WarehouseItem.Id), nameof(WarehouseItem.Timestamp), nameof(WarehouseItemModifyDto.CashRegCategoryId));
    }

    [Fact]
    public void ModifyDto_ApplyTo_leaves_identity_dates_concurrency_and_companies_untouched()
    {
        var created = new DateTime(2019, 5, 1);
        var rowVersion = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 };
        var mapping = new CompanyWarehouseItemMapping { WarehouseItemId = 42, CompanyId = 3 };
        var entity = new WarehouseItem
        {
            Id = 42, DateCreated = created, DateLastModified = created, Timestamp = rowVersion,
            CompanyMappings = new List<CompanyWarehouseItemMapping> { mapping }
        };
        var dto = MappingAssert.CreateFilled<WarehouseItemModifyDto>();
        dto.Id = 999;
        dto.Timestamp = [1, 1, 1, 1, 1, 1, 1, 1];

        dto.ApplyTo(entity);

        Assert.Equal(42, entity.Id);
        Assert.Equal(created, entity.DateCreated);
        Assert.Equal(created, entity.DateLastModified);
        Assert.Same(rowVersion, entity.Timestamp);
        Assert.Same(mapping, Assert.Single(entity.CompanyMappings));
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<WarehouseItem>();
        src.MainMeasureUnit = new MeasureUnit { Code = "MU" };
        src.BuyMeasureUnit = new MeasureUnit { Code = "BU" };
        src.FpaDef = new FpaDef { Name = "FPA24" };
        src.MaterialCaterory = new MaterialCategory { Name = "CAT" };
        src.Company = new Company { Code = "CO" };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, WarehouseItemMappings.ToListDto.Compile());

        Assert.Equal("MU", dto.MainMeasureUnitCode);
        Assert.Equal("BU", dto.BuyMeasureUnitCode);
        Assert.Equal("FPA24", dto.FpaDefName);
        Assert.Equal("CAT", dto.MaterialCategoryName);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Null(dto.Url);
    }

    [Fact]
    public void ToBigClass_takes_item_fields_and_the_mapping_company()
    {
        var item = MappingAssert.CreateFilled<WarehouseItem>();
        item.FpaDef = new FpaDef { Id = 77, Name = "FPA24" };
        item.MaterialCaterory = new MaterialCategory { Name = "CAT" };
        var src = new CompanyWarehouseItemMapping
        {
            CompanyId = 5, Company = new Company { Code = "C5" }, WarehouseItemId = item.Id, WarehouseItem = item
        };

        var dto = WarehouseItemMappings.ToBigClass.Compile()(src);

        Assert.Equal(item.Id, dto.Id);
        Assert.Equal(item.Code, dto.Code);
        Assert.Equal(item.Name, dto.Name);
        Assert.Equal(item.WarehouseItemNature, dto.WarehouseItemNature);
        Assert.Equal("77", dto.FpaDefName); // as AutoMapper had it
        Assert.Equal("CAT", dto.MaterialCategoryName);
        Assert.Equal(5, dto.CompanyId);
        Assert.Equal("C5", dto.CompanyCode);
    }

    [Fact]
    public void ToSearchListDto_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<WarehouseItem, WarehouseItemSearchListDto>(
            WarehouseItemMappings.ToSearchListDto.Compile());
    }
}
