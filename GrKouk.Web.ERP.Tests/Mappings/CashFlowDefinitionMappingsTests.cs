using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class CashFlowDefinitionMappingsTests
{
    private static Company Company() => new() { Name = "Main Co", Code = "C01" };

    [Fact]
    public void TransactionDef_ToListDto_copies_fields_and_company()
    {
        var src = MappingAssert.CreateFilled<CashFlowTransactionDef>();
        src.Company = Company();

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, CashFlowDefinitionMappings.TransactionDefToListDto.Compile());

        Assert.Equal("Main Co", dto.CompanyName);
        Assert.Equal("C01", dto.CompanyCode);
    }

    [Fact]
    public void DocTypeDef_ToListDto_copies_fields_and_flattens_transaction_def_and_company()
    {
        var src = MappingAssert.CreateFilled<CashFlowDocTypeDef>();
        src.CashFlowTransactionDefinition = new CashFlowTransactionDef { Name = "Receipt", Code = "RCV" };
        src.Company = Company();

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, CashFlowDefinitionMappings.DocTypeDefToListDto.Compile());

        Assert.Equal(src.CashFlowTransactionDefId, dto.CfaTransactionDefId);
        Assert.Equal("Receipt", dto.CfaTransactionDefName);
        Assert.Equal("RCV", dto.CfaTransactionDefCode);
        Assert.Equal("Main Co", dto.CompanyName);
        Assert.Equal("C01", dto.CompanyCode);
    }

    [Fact]
    public void DocSeriesDef_ToListDto_copies_fields_and_flattens_doc_type_and_company()
    {
        var src = MappingAssert.CreateFilled<CashFlowDocSeriesDef>();
        src.CashFlowDocTypeDefinition = new CashFlowDocTypeDef { Name = "Cash receipt", Code = "CR" };
        src.Company = Company();

        var dto = MappingAssert.AllMatchingPropertiesCopied(src, CashFlowDefinitionMappings.DocSeriesDefToListDto.Compile());

        Assert.Equal(src.CashFlowDocTypeDefId, dto.CfaDocTypeDefId);
        Assert.Equal("Cash receipt", dto.CfaDocTypeDefName);
        Assert.Equal("CR", dto.CfaDocTypeDefCode);
        Assert.Equal("Main Co", dto.CompanyName);
        Assert.Equal("C01", dto.CompanyCode);
    }

    private static ApiDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True;")
            .Options);

    [Fact]
    public void All_three_projections_translate_to_sql()
    {
        using var db = CreateContext();

        Assert.Contains("JOIN", db.CashFlowTransactionDefs.Select(CashFlowDefinitionMappings.TransactionDefToListDto).ToQueryString());
        Assert.Contains("JOIN", db.CashFlowDocTypeDefs.Select(CashFlowDefinitionMappings.DocTypeDefToListDto).ToQueryString());
        Assert.Contains("JOIN", db.CashFlowDocSeriesDefs.Select(CashFlowDefinitionMappings.DocSeriesDefToListDto).ToQueryString());
    }
}
