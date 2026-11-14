using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashRegister;
using GrKouk.Erp.Dtos.Media;
using GrKouk.Erp.Dtos.ProductRecipies;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

/// <summary>
/// The projections are Expressions used in EF queries. Here they are compiled and run
/// in memory to check the values; EfTranslationTests checks they translate to SQL.
/// </summary>
public class ListProjectionMappingsTests
{
    [Fact]
    public void WrItemCode_ToListDto_copies_fields_and_flattens_WarehouseItem()
    {
        var src = MappingAssert.CreateFilled<WrItemCode>();
        src.WarehouseItem = new WarehouseItem { Id = 77, Name = "Flour 1kg" };

        // Names filled after paging by the controller are not part of the projection.
        var dto = MappingAssert.AllMatchingPropertiesCopied(src, WrItemCodeMappings.ToListDto.Compile());

        Assert.Same(src.WarehouseItem, dto.WarehouseItem);
        Assert.Equal("Flour 1kg", dto.WarehouseItemName);
    }

    [Fact]
    public void MediaEntry_ToDto_copies_fields_including_differently_cased_OriginalFilename()
    {
        var src = MappingAssert.CreateFilled<MediaEntry>();

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, MediaEntryMappings.ToDto.Compile());

        Assert.Equal(src.OriginalFileName, dto.OriginalFilename);
        Assert.Null(dto.Url); // set by the controller
    }

    [Fact]
    public void ProductRecipe_ToDto_flattens_product_and_company_names()
    {
        var src = MappingAssert.CreateFilled<ProductRecipe>();
        src.Product = new WarehouseItem { Name = "Bread" };
        src.Company = new Company { Name = "Main Co" };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, ProductRecipeMappings.ToDto.Compile());

        Assert.Equal("Bread", dto.ProductName);
        Assert.Equal("Main Co", dto.CompanyName);
    }

    [Fact]
    public void CrCatWarehouseItem_ToListDto_flattens_related_names()
    {
        var src = MappingAssert.CreateFilled<CrCatWarehouseItem>();
        src.ClientProfile = new ClientProfile { Name = "POS 1", Company = new Company { Code = "C01" } };
        src.CashRegCategory = new CashRegCategory { Name = "Bakery" };
        src.WarehouseItem = new WarehouseItem { Name = "Croissant" };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, CrCatWarehouseItemMappings.ToListDto.Compile());

        Assert.Equal("POS 1", dto.ClientProfileName);
        Assert.Equal("C01", dto.CompanyCode);
        Assert.Equal("Bakery", dto.CashRegCategoryName);
        Assert.Equal("Croissant", dto.WarehouseItemName);
    }

    [Fact]
    public void TransWarehouseDef_ToListDto_copies_fields_and_company_name()
    {
        var src = MappingAssert.CreateFilled<TransWarehouseDef>();
        src.Company = new Company { Name = "Main Co" };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, TransWarehouseDefMappings.ToListDto.Compile());

        Assert.Equal("Main Co", dto.CompanyName);
    }
}
