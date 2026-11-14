using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.FinDiary;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class FinDiaryTransactionMappingsTests
{
    [Fact]
    public void Entity_ToExpenseModifyDto_copies_every_matching_property_and_the_timestamp()
    {
        var src = MappingAssert.CreateFilled<FinDiaryTransaction>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToExpenseModifyDto());

        Assert.Equal(src.Timestamp, dto.Timestamp);
    }

    [Fact]
    public void ModifyDto_ToEntity_copies_form_fields_but_not_id_kind_or_revenue_centre()
    {
        var dto = MappingAssert.CreateFilled<FinDiaryExpenceTransModifyDto>();

        var entity = MappingAssert.AllMatchingPropertiesCopied(dto, d => d.ToEntity(),
            nameof(FinDiaryTransaction.Id), nameof(FinDiaryTransaction.Kind),
            nameof(FinDiaryTransaction.RevenueCentreId), nameof(FinDiaryTransaction.Timestamp));

        // A "copy from" form carries the source Id; the new row must not reuse it.
        Assert.Equal(0, entity.Id);
        Assert.Equal(0, entity.Kind);            // set by the Create page
        Assert.Equal(0, entity.RevenueCentreId); // set by the Create page
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_editable_fields_and_keeps_kind()
    {
        var rowVersion = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 };
        var entity = new FinDiaryTransaction { Id = 42, Kind = 1, RevenueCentreId = 1, Timestamp = rowVersion };
        var dto = MappingAssert.CreateFilled<FinDiaryExpenceTransModifyDto>();
        dto.Kind = 0; // the Edit form does not post Kind

        dto.ApplyTo(entity);

        Assert.Equal(dto.TransactionDate, entity.TransactionDate);
        Assert.Equal(dto.ReferenceCode, entity.ReferenceCode);
        Assert.Equal(dto.TransactorId, entity.TransactorId);
        Assert.Equal(dto.FinTransCategoryId, entity.FinTransCategoryId);
        Assert.Equal(dto.CompanyId, entity.CompanyId);
        Assert.Equal(dto.CostCentreId, entity.CostCentreId);
        Assert.Equal(dto.Description, entity.Description);
        Assert.Equal(dto.AmountFpa, entity.AmountFpa);
        Assert.Equal(dto.AmountNet, entity.AmountNet);

        Assert.Equal(42, entity.Id);
        Assert.Equal(1, entity.Kind);
        Assert.Equal(1, entity.RevenueCentreId);
        Assert.Same(rowVersion, entity.Timestamp);
    }
}
