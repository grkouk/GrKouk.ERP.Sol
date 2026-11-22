using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Web.ERP.Pages.Transactions.BuyMaterialsDoc;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Tests.Mappings;

public class TextSearchTests
{
    private static ApiDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True;")
            .Options);

    [Fact]
    public void Words_are_split_lowercased_and_get_the_other_keyboard_layout()
    {
        var words = TextSearch.GetSearchWords("  ISAA  ψοψα ");

        Assert.Equal(2, words.Count);
        Assert.Equal(new[] { "isaa", "ισαα" }, words[0]);
        Assert.Equal(new[] { "ψοψα", "coca" }, words[1]);
    }

    [Fact]
    public void Accents_are_removed_before_swapping()
    {
        var words = TextSearch.GetSearchWords("Βασιλόπουλος");

        Assert.Single(words);
        Assert.Equal("βασιλοπουλος", words[0][0]);
        Assert.Equal("basilopoylow", words[0][1]);
    }

    [Fact]
    public void Words_without_letters_have_no_swapped_variant()
    {
        var words = TextSearch.GetSearchWords("1234");

        Assert.Equal(new[] { "1234" }, Assert.Single(words));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a β")]
    public void Nothing_to_search_gives_no_predicate(string text)
    {
        Assert.Empty(TextSearch.GetSearchWords(text));
        Assert.Null(TextSearch.BuildPredicate<string>(text, s => s));
    }

    [Fact]
    public void Repeated_words_are_searched_once()
    {
        Assert.Single(TextSearch.GetSearchWords("abc ABC"));
    }

    [Fact]
    public void Single_query_search_translates_to_accent_insensitive_like()
    {
        using var db = CreateContext();
        var query = db.BuyDocuments.AsNoTracking().Select(BuyDocumentMappings.ToList2Dto);

        var sql = TextSearch.Apply(query, "isaa 1234", p => p.TransactorName, p => p.TransRefCode)
            .ToQueryString();

        Assert.Contains("COLLATE Greek_CI_AI LIKE", sql);
        // Words are parameters, not literals: one cached plan for every search.
        Assert.DoesNotContain("isaa", sql.Split("SELECT")[^1]);
        Assert.Contains("AND", sql);
    }

    [Fact]
    public async Task Empty_search_leaves_the_query_unchanged()
    {
        using var db = CreateContext();
        var query = db.BuyDocuments.AsNoTracking();

        Assert.Same(query, TextSearch.Apply(query, " ", b => b.TransRefCode));
        // The two-step search returns before running any lookup query (no database here).
        Assert.Same(query, await IndexSfModel.SearchSpec(db).ApplyAsync(query, " a "));
    }
}
