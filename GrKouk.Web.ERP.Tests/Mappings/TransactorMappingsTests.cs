using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class TransactorMappingsTests
{
    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<TransactorCreateDto, Transactor>(d => d.ToEntity());

        Assert.Equal(0, entity.Id);
        Assert.Empty(entity.TransactorCompanyMappings); // added by the page from SelectedCompanies
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property_and_the_timestamp()
    {
        var src = MappingAssert.CreateFilled<Transactor>();
        src.Timestamp = [1, 2, 3, 4, 5, 6, 7, 8];

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToModifyDto());

        Assert.Equal(src.Timestamp, dto.Timestamp);
        Assert.Null(dto.SelectedCompanies); // set by the page
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_editable_fields()
    {
        // Start from an empty entity so every copied value is a change that can be detected.
        MappingAssert.AllMatchingPropertiesCopied<TransactorModifyDto, Transactor>(
            d => { var e = new Transactor(); d.ApplyTo(e); return e; },
            ignore: nameof(Transactor.Id));
    }

    [Fact]
    public void ModifyDto_ApplyTo_leaves_identity_dates_concurrency_and_companies_untouched()
    {
        var created = new DateTime(2019, 5, 1);
        var rowVersion = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 };
        var mapping = new TransactorCompanyMapping { TransactorId = 42, CompanyId = 3 };
        var entity = new Transactor
        {
            Id = 42,
            DateCreated = created,
            DateLastModified = created,
            Timestamp = rowVersion,
            TransactorCompanyMappings = new List<TransactorCompanyMapping> { mapping }
        };
        var dto = MappingAssert.CreateFilled<TransactorModifyDto>();
        dto.Id = 999;
        dto.Timestamp = [1, 1, 1, 1, 1, 1, 1, 1];

        dto.ApplyTo(entity);

        Assert.Equal(42, entity.Id);
        Assert.Equal(created, entity.DateCreated);
        Assert.Equal(created, entity.DateLastModified); // the page sets this, not the mapping
        Assert.Same(rowVersion, entity.Timestamp);
        Assert.Same(mapping, Assert.Single(entity.TransactorCompanyMappings));
        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void Entity_ToDetailDto_copies_fields_and_flattens_transactor_type()
    {
        var src = MappingAssert.CreateFilled<Transactor>();
        src.TransactorType = new TransactorType { Name = "Supplier", Code = "SUP" };

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, s => s.ToDetailDto());

        Assert.Equal("Supplier", dto.TransactorTypeName);
        Assert.Equal("SUP", dto.TransactorTypeCode);
        Assert.Null(dto.Companies); // set by the page
    }

    [Fact]
    public void Entity_ToDetailDto_tolerates_missing_transactor_type()
    {
        var dto = new Transactor { Id = 1, Name = "X" }.ToDetailDto();

        Assert.Null(dto.TransactorTypeName);
        Assert.Null(dto.TransactorTypeCode);
    }
}
