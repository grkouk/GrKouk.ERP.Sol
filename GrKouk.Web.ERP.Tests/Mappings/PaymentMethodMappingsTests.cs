using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.PaymentMethods;
using GrKouk.Web.ERP.Mappings;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class PaymentMethodMappingsTests
{
    [Fact]
    public void CreateDto_ToEntity_copies_every_matching_property()
    {
        var entity = MappingAssert.AllMatchingPropertiesCopied<PaymentMethodCreateDto, PaymentMethod>(d => d.ToEntity());

        Assert.Equal(0, entity.Id);
        Assert.Equal(default, entity.CreatedAt); // set by the page
    }

    [Fact]
    public void Entity_ToModifyDto_copies_every_matching_property()
    {
        MappingAssert.AllMatchingPropertiesCopied<PaymentMethod, PaymentMethodModifyDto>(e => e.ToModifyDto());
    }

    [Fact]
    public void ModifyDto_ApplyTo_copies_editable_fields()
    {
        MappingAssert.AllMatchingPropertiesCopied<PaymentMethodModifyDto, PaymentMethod>(
            d => { var e = new PaymentMethod(); d.ApplyTo(e); return e; },
            nameof(PaymentMethod.Id), nameof(PaymentMethod.CreatedAt), nameof(PaymentMethod.ModifiedAt));
    }

    [Fact]
    public void ModifyDto_ApplyTo_leaves_identity_and_dates_untouched()
    {
        // The Edit form does not post the dates, so the bound DTO carries DateTime.MinValue.
        var created = new DateTime(2019, 5, 1);
        var entity = new PaymentMethod { Id = 7, CreatedAt = created, ModifiedAt = created, CompanyId = 1 };
        var dto = new PaymentMethodModifyDto
        {
            Id = 999, Code = "CASH", Name = "Cash", Active = true, CompanyId = 3,
            AutoPayoffWay = SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto
        };

        dto.ApplyTo(entity);

        Assert.Equal(7, entity.Id);
        Assert.Equal(created, entity.CreatedAt);
        Assert.Equal(created, entity.ModifiedAt); // the page sets this, not the mapping
        Assert.Equal(3, entity.CompanyId);
        Assert.True(entity.Active);
        Assert.Equal(SeriesAutoPayoffEnum.SeriesAutoPayoffEnumAuto, entity.AutoPayoffWay);
    }
}
