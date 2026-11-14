using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class WarehouseTransactionMappingsTests
{
    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<WarehouseTransCreateDto, WarehouseTransaction>(
            d => d.ToEntity(), nameof(WarehouseTransaction.Timestamp));

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property_and_the_timestamp()
    {
        var src = MappingAssert.CreateFilled<WarehouseTransaction>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToModifyDto());

        Assert.Equal(src.Timestamp, dto.Timestamp);
        Assert.Null(dto.PrimaryUnitCode); // set by the page
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_posted_fields_and_keeps_the_rest()
    {
        var rowVersion = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 };
        var entity = new WarehouseTransaction
        {
            Id = 42, CreatorId = 7, SectionId = 3,
            InvoicedVolumeAction = InventoryActionEnum.InventoryActionEnumImport,
            InvoicedValueAction = InventoryValueActionEnum.InventoryValueActionEnumIncrease,
            DiscountRate = 0.1m, UnitExpenses = 2m, AmountExpenses = 4m, TransExpensesAmount = 4m,
            Timestamp = rowVersion
        };
        var dto = MappingAssert.CreateFilled<WarehouseTransModifyDto>();

        dto.ApplyTo(entity);

        Assert.Equal(dto.TransDate, entity.TransDate);
        Assert.Equal(dto.TransWarehouseDocSeriesId, entity.TransWarehouseDocSeriesId);
        Assert.Equal(dto.TransRefCode, entity.TransRefCode);
        Assert.Equal(dto.WarehouseItemId, entity.WarehouseItemId);
        Assert.Equal(dto.FiscalPeriodId, entity.FiscalPeriodId);
        Assert.Equal(dto.PrimaryUnitId, entity.PrimaryUnitId);
        Assert.Equal(dto.SecondaryUnitId, entity.SecondaryUnitId);
        Assert.Equal(dto.UnitFactor, entity.UnitFactor);
        Assert.Equal(dto.Quontity1, entity.Quontity1);
        Assert.Equal(dto.Quontity2, entity.Quontity2);
        Assert.Equal(dto.FpaRate, entity.FpaRate);
        Assert.Equal(dto.UnitPrice, entity.UnitPrice);
        Assert.Equal(dto.AmountFpa, entity.AmountFpa);
        Assert.Equal(dto.AmountNet, entity.AmountNet);
        Assert.Equal(dto.AmountDiscount, entity.AmountDiscount);
        Assert.Equal(dto.Etiology, entity.Etiology);
        Assert.Equal(dto.CompanyId, entity.CompanyId);

        // Not posted by the Edit form: the old Attach+Modified save reset these to 0.
        Assert.Equal(42, entity.Id);
        Assert.Equal(7, entity.CreatorId);
        Assert.Equal(3, entity.SectionId);
        Assert.Equal(InventoryActionEnum.InventoryActionEnumImport, entity.InvoicedVolumeAction);
        Assert.Equal(InventoryValueActionEnum.InventoryValueActionEnumIncrease, entity.InvoicedValueAction);
        Assert.Equal(0.1m, entity.DiscountRate);
        Assert.Equal(2m, entity.UnitExpenses);
        Assert.Equal(4m, entity.AmountExpenses);
        Assert.Equal(4m, entity.TransExpensesAmount);
        Assert.Same(rowVersion, entity.Timestamp);
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<WarehouseTransaction>();
        src.TransWarehouseDocSeries = new() { Name = "SerName", Code = "SER" };
        src.Section = new Section { Code = "SEC" };
        src.WarehouseItem = new WarehouseItem { Name = "Item" };
        src.Company = new Company { Code = "CO", CurrencyId = 3 };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, WarehouseTransactionMappings.ToListDto.Compile());

        Assert.Equal("SerName", dto.TransWarehouseDocSeriesName);
        Assert.Equal("SER", dto.TransWarehouseDocSeriesCode);
        Assert.Equal("SEC", dto.SectionCode);
        Assert.Equal("Item", dto.WarehouseItemName);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Equal(3, dto.CompanyCurrencyId);
    }
}
