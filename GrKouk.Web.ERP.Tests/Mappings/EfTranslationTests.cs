using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Tests.Mappings;

/// <summary>
/// Checks that each projection translates to SQL with the real SQL Server provider.
/// ToQueryString() runs EF's translation without opening a connection, so no database is needed.
/// A projection that EF cannot translate throws here instead of at runtime.
/// </summary>
public class EfTranslationTests
{
    private static ApiDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True;")
            .Options);

    [Fact]
    public void SellDocTypeDef_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.SellDocTypeDefs.Select(SellDocTypeDefMappings.ToListDto).ToQueryString();
        Assert.Contains("LEFT JOIN", sql);
    }

    [Fact]
    public void WrItemCode_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.WrItemCodes.Select(WrItemCodeMappings.ToListDto).ToQueryString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void MediaEntry_ToDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.MediaEntries.Select(MediaEntryMappings.ToDto).ToQueryString();
        Assert.Contains("[OriginalFileName]", sql);
    }

    [Fact]
    public void ProductRecipe_ToDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.ProductRecipes.Select(ProductRecipeMappings.ToDto).ToQueryString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void CrCatWarehouseItem_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.CrCatWarehouseItems.Select(CrCatWarehouseItemMappings.ToListDto).ToQueryString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void TransWarehouseDef_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.TransWarehouseDefs.Select(TransWarehouseDefMappings.ToListDto).ToQueryString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void CfaTransaction_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.CashFlowAccountTransactions.Select(CfaTransactionMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }

    [Fact]
    public void WarehouseItem_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.WarehouseItems.Select(WarehouseItemMappings.ToListDto).ToQueryString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void WarehouseItem_ToBigClass_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.CompanyWarehouseItemMappings.Select(WarehouseItemMappings.ToBigClass).ToQueryString();
        Assert.Contains("CONVERT", sql);
    }

    [Fact]
    public void WarehouseItem_ToSearchListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.WarehouseItems.Select(WarehouseItemMappings.ToSearchListDto).ToQueryString();
        Assert.Contains("[WarehouseItemNature]", sql);
    }

    [Fact]
    public void WarehouseTransaction_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.WarehouseTransactions.Select(WarehouseTransactionMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }

    [Fact]
    public void TransactorTransaction_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.TransactorTransactions.Select(TransactorTransactionMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }

    [Fact]
    public void BuyDocument_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.BuyDocuments.Select(BuyDocumentMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }

    [Fact]
    public void SellDocument_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.SellDocuments.Select(SellDocumentMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }

    [Fact]
    public void RecurringDocument_ToListDto_translates_to_sql()
    {
        using var db = CreateContext();
        var sql = db.RecurringTransDocs.Select(RecurringDocumentMappings.ToListDto).ToQueryString();
        Assert.Contains("[CurrencyId]", sql);
    }
}
