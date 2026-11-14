using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class TransactorTransactionMappingsTests
{
    [Fact]
    public void Entity_ToCreateDto_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied(MappingAssert.CreateFilled<TransactorTransaction>(), s => s.ToCreateDto());
    }

    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<TransactorTransCreateDto, TransactorTransaction>(
            d => d.ToEntity(), nameof(TransactorTransaction.Timestamp));

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property_and_the_timestamp()
    {
        var src = MappingAssert.CreateFilled<TransactorTransaction>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToModifyDto());

        Assert.Equal(src.Timestamp, dto.Timestamp);
    }

    [Fact]
    public void ToListDto_flattens_navigations()
    {
        var src = MappingAssert.CreateFilled<TransactorTransaction>();
        src.TransTransactorDocSeries = new() { Name = "SerName", Code = "SER" };
        src.Transactor = new Transactor { Name = "Supplier" };
        src.Section = new Section { Code = "SEC" };
        src.Company = new Company { Code = "CO", CurrencyId = 3 };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, TransactorTransactionMappings.ToListDto.Compile());

        Assert.Equal("SerName", dto.TransTransactorDocSeriesName);
        Assert.Equal("SER", dto.TransTransactorDocSeriesCode);
        Assert.Equal("Supplier", dto.TransactorName);
        Assert.Equal("SEC", dto.SectionCode);
        Assert.Equal("CO", dto.CompanyCode);
        Assert.Equal(3, dto.CompanyCurrencyId);
        Assert.Null(dto.CreatorSectionCode);
    }
}
