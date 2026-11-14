using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Warehouses;
using GrKouk.Erp.Dtos.Warehouses;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class WarehouseMappingsTests
{
    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<WarehouseCreateDto, Warehouse>(d => d.ToEntity());

        Assert.Equal(0, entity.Id);
        Assert.Empty(entity.CompanyMappings); // added by the page from SelectedCompanies
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property()
    {
        var dto = MappingAssert.AllMatchingPropertiesCopied<Warehouse, WarehouseModifyDto>(e => e.ToModifyDto());

        Assert.Null(dto.SelectedCompanies); // set by the page
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_editable_fields()
    {
        MappingAssert.AllMatchingPropertiesCopied<WarehouseModifyDto, Warehouse>(
            d => { var e = new Warehouse(); d.ApplyTo(e); return e; },
            nameof(Warehouse.Id), nameof(Warehouse.DateCreated), nameof(Warehouse.DateLastModified));
    }

    [Fact]
    public void ModifyDto_ApplyTo_leaves_identity_dates_and_companies_untouched()
    {
        // The Edit form does not post DateCreated, so the bound DTO carries DateTime.MinValue.
        var created = new DateTime(2019, 5, 1);
        var mapping = new WarehouseCompanyMapping { WarehouseId = 7, CompanyId = 3 };
        var entity = new Warehouse
        {
            Id = 7,
            DateCreated = created,
            DateLastModified = created,
            CompanyMappings = new List<WarehouseCompanyMapping> { mapping }
        };
        var dto = new WarehouseModifyDto { Id = 999, Code = "C2", Name = "Renamed" };

        dto.ApplyTo(entity);

        Assert.Equal(7, entity.Id);
        Assert.Equal(created, entity.DateCreated);
        Assert.Equal(created, entity.DateLastModified); // the page sets this, not the mapping
        Assert.Same(mapping, Assert.Single(entity.CompanyMappings));
        Assert.Equal("C2", entity.Code);
        Assert.Equal("Renamed", entity.Name);
    }
}
