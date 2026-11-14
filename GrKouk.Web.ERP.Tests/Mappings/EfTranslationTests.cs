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
}
