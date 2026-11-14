using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class RecurringDocumentMappingsTests
{
    [Fact]
    public void CreateAjaxDto_ToEntity_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<RecurringTransDocCreateAjaxDto, RecurringTransDoc>(
            d => d.ToEntity(), nameof(RecurringTransDoc.DocLines), nameof(RecurringTransDoc.Timestamp));
    }

    [Fact]
    public void ModifyAjaxDto_ToEntity_copies_every_matching_property()
    {
        var dto = MappingAssert.CreateFilled<RecurringTransDocModifyAjaxDto>();
        dto.Timestamp = [1, 2, 3];

        var entity = MappingAssert.AllMatchingPropertiesCopied(dto, d => d.ToEntity());

        Assert.Same(dto.Timestamp, entity.Timestamp);
        Assert.Equal(0, entity.DocTypeId); // set by the caller
    }

    [Fact]
    public void ToModifyDto_maps_header_and_lines()
    {
        var src = MappingAssert.CreateFilled<RecurringTransDoc>();
        src.Transactor = new Transactor { Name = "T" };
        src.Company = new Company { Code = "CO" };
        var line = MappingAssert.CreateFilled<RecurringTransDocLine>();
        line.WarehouseItem = new WarehouseItem { Name = "Item" };
        src.DocLines = [line];

        var dto = src.ToModifyDto();

        Assert.Equal("T", dto.TransactorName);
        Assert.Equal("CO", dto.CompanyCode);
        var l = Assert.Single(dto.BuyDocLines);
        Assert.Equal("Item", l.WarehouseItemName);
        Assert.Equal(line.RecurringTransDocId, l.RecurringTransDocId);
        Assert.Equal(line.UnitPrice, l.UnitPrice);
    }

    [Fact]
    public void BuyDocument_ToRecurringModifyDto_uses_the_buy_series_and_lines()
    {
        var src = MappingAssert.CreateFilled<BuyDocument>();
        src.BuyDocLines = [MappingAssert.CreateFilled<BuyDocLine>()];

        var dto = src.ToRecurringModifyDto();

        Assert.Equal(src.BuyDocSeriesId, dto.DocSeriesId);
        Assert.Equal(src.SectionId, dto.SectionId);
        Assert.Single(dto.BuyDocLines);
        Assert.Null(dto.RecurringFrequency);
    }

    [Fact]
    public void SellDocument_ToRecurringModifyDto_uses_the_sell_series_and_lines()
    {
        var src = MappingAssert.CreateFilled<SellDocument>();
        src.SellDocLines = [MappingAssert.CreateFilled<SellDocLine>(), MappingAssert.CreateFilled<SellDocLine>()];

        var dto = src.ToRecurringModifyDto();

        Assert.Equal(src.SellDocSeriesId, dto.DocSeriesId);
        Assert.Equal(2, dto.BuyDocLines.Count);
    }

    [Fact]
    public void ToBuyDocCreateAjaxDto_uses_next_date_series_and_converts_lines()
    {
        var src = MappingAssert.CreateFilled<RecurringTransDoc>();
        var line = MappingAssert.CreateFilled<RecurringTransDocLine>();
        line.FpaRate = 0.24m; line.DiscountRate = 0.1m;
        src.DocLines = [line];

        var dto = src.ToBuyDocCreateAjaxDto();

        Assert.Equal(0, dto.Id);
        Assert.Equal(src.NextTransDate, dto.TransDate);
        Assert.Equal(src.DocSeriesId, dto.BuyDocSeriesId);
        var l = Assert.Single(dto.BuyDocLines);
        Assert.Equal(line.Quontity1, l.Q1);
        Assert.Equal(line.Quontity2, l.Q2);
        Assert.Equal(line.UnitPrice, l.Price);
        Assert.Equal(line.PrimaryUnitId, l.MainUnitId);
        Assert.Equal(line.SecondaryUnitId, l.SecUnitId);
        Assert.Equal(0.24f, l.FpaRate);
        Assert.Equal(0.1f, l.DiscountRate);
    }

    [Fact]
    public void ToSellDocCreateAjaxDto_uses_next_date_series_and_converts_lines()
    {
        var src = MappingAssert.CreateFilled<RecurringTransDoc>();
        src.DocLines = [MappingAssert.CreateFilled<RecurringTransDocLine>()];

        var dto = src.ToSellDocCreateAjaxDto();

        Assert.Equal(src.NextTransDate, dto.TransDate);
        Assert.Equal(src.DocSeriesId, dto.SellDocSeriesId);
        Assert.Equal(0, dto.SalesChannelId);
        Assert.Single(dto.SellDocLines);
    }

    [Fact]
    public void ToListDto_flattens_transactor_and_company()
    {
        var src = MappingAssert.CreateFilled<RecurringTransDoc>();
        src.Transactor = new Transactor { Name = "T" };
        src.Company = new Company { Code = "CO", CurrencyId = 3 };

        var dto = RecurringDocumentMappings.ToListDto.Compile()(src);

        Assert.Equal("T", dto.TransactorName);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Equal(3, dto.CompanyCurrencyId);
        Assert.Equal(src.DocSeriesId, dto.DocSeriesId);
    }
}
