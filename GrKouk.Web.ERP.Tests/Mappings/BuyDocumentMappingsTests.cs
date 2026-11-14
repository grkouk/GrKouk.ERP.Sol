using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class BuyDocumentMappingsTests
{
    [Fact]
    public void CreateAjaxDto_ToEntity_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<BuyDocCreateAjaxDto, BuyDocument>(
            d => d.ToEntity(), nameof(BuyDocument.BuyDocLines));
    }

    [Fact]
    public void ModifyAjaxDto_ToEntity_copies_every_matching_property_except_section()
    {
        var dto = MappingAssert.CreateFilled<BuyDocModifyAjaxDto>();
        dto.Timestamp = [1, 2, 3];

        var entity = dto.ToEntity();

        Assert.Equal(dto.Id, entity.Id);
        Assert.Equal(dto.BuyDocTypeId, entity.BuyDocTypeId);
        Assert.Equal(dto.FiscalPeriodId, entity.FiscalPeriodId);
        Assert.Equal(dto.PaymentMethodId, entity.PaymentMethodId);
        Assert.Same(dto.Timestamp, entity.Timestamp);
        Assert.Equal(0, entity.SectionId); // set by the caller, as before
    }

    [Fact]
    public void CreateAjaxDto_ToTransactorTransaction_does_not_copy_the_document_id()
    {
        var dto = MappingAssert.CreateFilled<BuyDocCreateAjaxDto>();

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
        MappingAssert.AllMatchingPropertiesCopied(MappingAssert.CreateFilled<BuyDocModifyAjaxDto>(),
            d => d.ToTransactorTransCreateDto());
    }

    [Fact]
    public void ToModifyDto_flattens_navigations_and_maps_lines()
    {
        var src = MappingAssert.CreateFilled<BuyDocument>();
        src.Section = new Section { Code = "SEC" };
        src.Transactor = new Transactor { Name = "Sup" };
        src.FiscalPeriod = new FiscalPeriod { Code = "FP" };
        src.BuyDocSeries = new() { Code = "SER", Name = "SerName" };
        src.BuyDocType = new() { Code = "TYP", Name = "TypName" };
        src.Company = new Company { Code = "CO" };
        var line = MappingAssert.CreateFilled<BuyDocLine>();
        line.WarehouseItem = new WarehouseItem { Name = "Item" };
        src.BuyDocLines = [line];

        var dto = src.ToModifyDto();

        Assert.Equal("SEC", dto.SectionCode);
        Assert.Equal("Sup", dto.TransactorName);
        Assert.Equal("FP", dto.FiscalPeriodCode);
        Assert.Equal("SerName", dto.BuyDocSeriesName);
        Assert.Equal("TYP", dto.BuyDocTypeCode);
        Assert.Equal("CO", dto.CompanyCode);
        var l = Assert.Single(dto.BuyDocLines);
        Assert.Equal("Item", l.WarehouseItemName);
        MappingAssert.AllMatchingPropertiesCopied(line, x => x.ToModifyDto());
    }

    [Fact]
    public void ToModifyDto_tolerates_missing_navigations()
    {
        var src = MappingAssert.CreateFilled<BuyDocument>();
        src.BuyDocLines = [MappingAssert.CreateFilled<BuyDocLine>()];

        var dto = src.ToModifyDto();

        Assert.Null(dto.SectionCode);
        Assert.Null(Assert.Single(dto.BuyDocLines).WarehouseItemName);
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<BuyDocument>();
        src.Section = new Section { Code = "SEC" };
        src.Transactor = new Transactor { Name = "Sup" };
        src.BuyDocSeries = new() { Code = "SER", Name = "SerName" };
        src.Company = new Company { Code = "CO", CurrencyId = 3 };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, BuyDocumentMappings.ToListDto.Compile());

        Assert.Equal("SER", dto.BuyDocSeriesCode);
        Assert.Equal(3, dto.CompanyCurrencyId);
    }
}
