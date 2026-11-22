using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// The search box of the index pages: Greek/English aware, word based, accent insensitive.
/// <list type="bullet">
/// <item>The search text is split into words; every word must be found in at least one of
/// the given text columns.</item>
/// <item>A word matches as typed or swapped to the other keyboard layout
/// (<see cref="KeyboardTransliterator"/>), so "isaa" finds "ΙΣΑΑΚ" and "ψοψα" finds "COCA".</item>
/// <item>Accents are ignored on both sides: the word is stripped of them and the columns are
/// compared with the <see cref="Collation"/> collation (the database's own, Greek_CI_AS, is
/// accent sensitive). Case is ignored by the collation.</item>
/// <item>One-letter words are ignored (they would match almost everything).</item>
/// </list>
/// </summary>
public static class TextSearch
{
    public const string Collation = "Greek_CI_AI";

    private static readonly MethodInfo ContainsMethod =
        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

    private static readonly MethodInfo CollateMethod = typeof(RelationalDbFunctionsExtensions)
        .GetMethod(nameof(RelationalDbFunctionsExtensions.Collate))!
        .MakeGenericMethod(typeof(string));

    /// <summary>
    /// Filters the query to the rows where every search word is found in one of the columns.
    /// No words (empty text, or only one-letter words) leaves the query as it is.
    /// </summary>
    public static IQueryable<T> Apply<T>(IQueryable<T> query, string searchText,
        params Expression<Func<T, string>>[] columns)
    {
        var predicate = BuildPredicate(searchText, columns);
        return predicate == null ? query : query.Where(predicate);
    }

    /// <summary>The predicate <see cref="Apply{T}"/> uses; null when there is nothing to search.</summary>
    public static Expression<Func<T, bool>> BuildPredicate<T>(string searchText,
        params Expression<Func<T, string>>[] columns)
    {
        var words = GetSearchWords(searchText);
        if (words.Count == 0 || columns.Length == 0)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(T), "p");
        var collatedColumns = CollateColumns(columns, parameter);

        Expression allWords = null;
        foreach (var variants in words)
        {
            var anyColumn = MatchAnyColumn(collatedColumns, variants);
            allWords = allWords == null ? anyColumn : Expression.AndAlso(allWords, anyColumn);
        }

        return Expression.Lambda<Func<T, bool>>(allWords!, parameter);
    }

    /// <summary>
    /// Starts a search definition for <typeparamref name="T"/> that can also search the text
    /// columns of related (lookup) tables, matched on their own first. See <see cref="TextSearchSpec{T}"/>.
    /// </summary>
    public static TextSearchSpec<T> For<T>() => new();

    /// <summary>The columns, on the given parameter, compared with the search collation.</summary>
    internal static List<Expression> CollateColumns<T>(IEnumerable<Expression<Func<T, string>>> columns,
        ParameterExpression parameter) =>
        columns
            .Select(c => (Expression)Expression.Call(CollateMethod,
                Expression.Constant(EF.Functions),
                ReplaceParameter(c, parameter),
                Expression.Constant(Collation)))
            .ToList();

    /// <summary>column1 LIKE %variant1% OR column1 LIKE %variant2% OR column2 ...; null without columns.</summary>
    internal static Expression MatchAnyColumn(IEnumerable<Expression> collatedColumns, string[] variants)
    {
        Expression anyColumn = null;
        foreach (var column in collatedColumns)
        {
            foreach (var variant in variants)
            {
                var contains = Expression.Call(column, ContainsMethod, AsParameter(variant));
                anyColumn = anyColumn == null ? contains : Expression.OrElse(anyColumn, contains);
            }
        }
        return anyColumn;
    }

    internal static Expression ReplaceParameter(LambdaExpression lambda, ParameterExpression parameter) =>
        new ParameterReplacer(lambda.Parameters[0], parameter).Visit(lambda.Body);

    /// <summary>
    /// The words to search for, each with its variants: the word without accents, plus its
    /// keyboard-swapped form when that differs (words without letters, like "1234", have none).
    /// </summary>
    public static List<string[]> GetSearchWords(string searchText)
    {
        var result = new List<string[]>();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return result;
        }

        var words = RemoveDiacritics(searchText)
            .ToLowerInvariant()
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1)
            .Distinct();

        foreach (var word in words)
        {
            var swapped = KeyboardTransliterator.Transliterate(word);
            result.Add(swapped != word ? new[] { word, swapped } : new[] { word });
        }
        return result;
    }

    /// <summary>"Παπαδόπουλος" -> "Παπαδοπουλος", "ΐ" -> "ι".</summary>
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // A member access on a captured object makes EF send the value as a SQL parameter
    // (one cached plan for every search) instead of inlining it as a literal.
    internal static Expression AsParameter<TValue>(TValue value) =>
        Expression.Property(Expression.Constant(new SearchValue<TValue>(value)), nameof(SearchValue<TValue>.Value));

    private sealed class SearchValue<TValue>
    {
        public SearchValue(TValue value) => Value = value;
        public TValue Value { get; }
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _from ? _to : base.VisitParameter(node);
    }
}
