using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class CfaTransactionMappingsTests
{
    [Fact]
    public void Entity_ToCreateDto_copies_matching_and_renamed_properties()
    {
        var src = MappingAssert.CreateFilled<CashFlowAccountTransaction>();

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToCreateDto());

        Assert.Equal(src.DocumentSeriesId, dto.DocSeriesId);
        Assert.Equal(src.DocumentTypeId, dto.DocTypeId);
        Assert.Equal(src.RefCode, dto.TransRefCode);
    }

    [Fact]
    public void CreateDto_ToEntity_copies_matching_and_renamed_properties()
    {
        var dto = MappingAssert.CreateFilled<CfaTransactionCreateDto>();

        var entity = MappingAssert.AllMatchingPropertiesCopied(dto, d => d.ToEntity());

        Assert.Equal(0, entity.Id);
        Assert.Equal(dto.DocSeriesId, entity.DocumentSeriesId);
        Assert.Equal(dto.DocTypeId, entity.DocumentTypeId);
        Assert.Equal(dto.TransRefCode, entity.RefCode);
    }

    [Fact]
    public void Entity_ToModifyDto_copies_matching_and_renamed_properties_and_timestamp()
    {
        var src = MappingAssert.CreateFilled<CashFlowAccountTransaction>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToModifyDto());

        Assert.Equal(src.Timestamp, dto.Timestamp);
        Assert.Equal(src.DocumentSeriesId, dto.DocSeriesId);
        Assert.Equal(src.DocumentTypeId, dto.DocTypeId);
        Assert.Equal(src.RefCode, dto.TransRefCode);
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_posted_fields_and_keeps_the_rest()
    {
        var rowVersion = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 };
        var entity = new CashFlowAccountTransaction
        {
            Id = 42,
            CreatorId = 7,
            CreatorSectionId = 8,
            SectionId = 3,
            DocumentTypeId = 4,
            FiscalPeriodId = 5,
            CfaAction = GrKouk.Erp.Definitions.CashFlowAccountActionsEnum.CfaActionDeposit,
            TransAmount = 10m,
            Timestamp = rowVersion
        };
        var dto = MappingAssert.CreateFilled<CfaTransactionModifyDto>();

        dto.ApplyTo(entity);

        Assert.Equal(dto.TransDate, entity.TransDate);
        Assert.Equal(dto.DocSeriesId, entity.DocumentSeriesId);
        Assert.Equal(dto.TransRefCode, entity.RefCode);
        Assert.Equal(dto.CashFlowAccountId, entity.CashFlowAccountId);
        Assert.Equal(dto.Amount, entity.Amount);
        Assert.Equal(dto.Etiology, entity.Etiology);
        Assert.Equal(dto.CompanyId, entity.CompanyId);

        // Not posted by the Edit form (the old Attach+Modified reset Creator* to 0).
        Assert.Equal(42, entity.Id);
        Assert.Equal(7, entity.CreatorId);
        Assert.Equal(8, entity.CreatorSectionId);
        Assert.Same(rowVersion, entity.Timestamp);
        // Recalculated by the page.
        Assert.Equal(3, entity.SectionId);
        Assert.Equal(4, entity.DocumentTypeId);
        Assert.Equal(5, entity.FiscalPeriodId);
        Assert.Equal(10m, entity.TransAmount);
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<CashFlowAccountTransaction>();
        src.DocumentSeries = new CashFlowDocSeriesDef { Name = "Series", Code = "SER" };
        src.CashFlowAccount = new CashFlowAccount { Name = "Cash" };
        src.Section = new Section { Code = "SEC" };
        src.Company = new Company { Code = "CO", CurrencyId = 2 };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, CfaTransactionMappings.ToListDto.Compile());

        Assert.Equal(src.DocumentSeriesId, dto.DocSeriesId);
        Assert.Equal(src.DocumentTypeId, dto.DocTypeId);
        Assert.Equal(src.RefCode, dto.TransRefCode);
        Assert.Equal("Series", dto.DocSeriesName);
        Assert.Equal("SER", dto.DocSeriesCode);
        Assert.Equal("Cash", dto.CashFlowAccountName);
        Assert.Equal("SEC", dto.SectionCode);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Equal(2, dto.CompanyCurrencyId);
        Assert.Null(dto.CreatorSectionCode); // filled by the controller
    }
}
