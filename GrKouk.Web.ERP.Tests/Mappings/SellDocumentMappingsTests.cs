using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class SellDocumentMappingsTests
{
    [Fact]
    public void CreateAjaxDto_ToEntity_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<SellDocCreateAjaxDto, SellDocument>(
            d => d.ToEntity(), nameof(SellDocument.SellDocLines));
    }

    [Fact]
    public void ModifyAjaxDto_ToEntity_copies_every_matching_property_except_section()
    {
        var dto = MappingAssert.CreateFilled<SellDocModifyAjaxDto>();
        dto.Timestamp = [1, 2, 3];

        var entity = dto.ToEntity();

        Assert.Equal(dto.Id, entity.Id);
        Assert.Equal(dto.SellDocTypeId, entity.SellDocTypeId);
        Assert.Equal(dto.FiscalPeriodId, entity.FiscalPeriodId);
        Assert.Equal(dto.PaymentMethodId, entity.PaymentMethodId);
        Assert.Equal(dto.SalesChannelId, entity.SalesChannelId);
        Assert.Same(dto.Timestamp, entity.Timestamp);
        Assert.Equal(0, entity.SectionId); // set by the caller, as before
    }

    [Fact]
    public void CreateAjaxDto_ToTransactorTransaction_does_not_copy_the_document_id()
    {
        var dto = MappingAssert.CreateFilled<SellDocCreateAjaxDto>();

        var tt = dto.ToTransactorTransaction();

        Assert.Equal(0, tt.Id);
        Assert.Equal(dto.TransDate, tt.TransDate);
        Assert.Equal(dto.TransRefCode, tt.TransRefCode);
        Assert.Equal(dto.TransactorId, tt.TransactorId);
        Assert.Equal(dto.AmountNet, tt.AmountNet);
        Assert.Equal(dto.AmountFpa, tt.AmountFpa);
        Assert.Equal(dto.AmountDiscount, tt.AmountDiscount);
        Assert.Equal(dto.Etiology, tt.Etiology);
        Assert.Equal(dto.CompanyId, tt.CompanyId);
    }

    [Fact]
    public void ModifyAjaxDto_ToTransactorTransCreateDto_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied(MappingAssert.CreateFilled<SellDocModifyAjaxDto>(),
            d => d.ToTransactorTransCreateDto());
    }

    [Fact]
    public void ToModifyDto_flattens_navigations_and_maps_lines()
    {
        var src = MappingAssert.CreateFilled<SellDocument>();
        src.Section = new Section { Code = "SEC" };
        src.Transactor = new Transactor { Name = "Cust" };
        src.FiscalPeriod = new FiscalPeriod { Code = "FP" };
        src.SellDocSeries = new() { Code = "SER", Name = "SerName" };
        src.SellDocType = new() { Code = "TYP", Name = "TypName" };
        src.Company = new Company { Code = "CO" };
        var line = MappingAssert.CreateFilled<SellDocLine>();
        line.WarehouseItem = new WarehouseItem { Name = "Item", WarehouseItemNature = (WarehouseItemNatureEnum)2 };
        src.SellDocLines = [line];

        var dto = src.ToModifyDto();

        Assert.Equal("SEC", dto.SectionCode);
        Assert.Equal("Cust", dto.TransactorName);
        Assert.Equal("FP", dto.FiscalPeriodCode);
        Assert.Equal("SerName", dto.SellDocSeriesName);
        Assert.Equal("TYP", dto.SellDocTypeCode);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Equal(src.SalesChannelId, dto.SalesChannelId);
        var l = Assert.Single(dto.SellDocLines);
        Assert.Equal("Item", l.WarehouseItemName);
        Assert.Equal(2, l.WarehouseItemNature);
        MappingAssert.AllMatchingPropertiesCopied(line, x => x.ToModifyDto());
    }

    [Fact]
    public void ToModifyDto_tolerates_missing_navigations()
    {
        var src = MappingAssert.CreateFilled<SellDocument>();
        src.SellDocLines = [MappingAssert.CreateFilled<SellDocLine>()];

        var dto = src.ToModifyDto();

        Assert.Null(dto.SectionCode);
        var l = Assert.Single(dto.SellDocLines);
        Assert.Null(l.WarehouseItemName);
        Assert.Equal(0, l.WarehouseItemNature);
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<SellDocument>();
        src.Section = new Section { Code = "SEC" };
        src.Transactor = new Transactor { Name = "Cust" };
        src.SellDocSeries = new() { Code = "SER", Name = "SerName" };
        src.Company = new Company { Code = "CO", CurrencyId = 3 };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, SellDocumentMappings.ToListDto.Compile());

        Assert.Equal("SER", dto.SellDocSeriesCode);
        Assert.Equal(3, dto.CompanyCurrencyId);
    }
}
