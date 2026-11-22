using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// A <see cref="TextSearch"/> over a large table whose text comes partly from small related
/// tables (transactor name, series, company...). Same matching rules, but a lookup table is
/// searched on its own first and the main rows are then filtered by the matching ids:
/// <c>TransactorId IN (...) OR ... OR TransRefCode LIKE ...</c>.
/// Testing every joined column on every main row is several times slower (measured on buy
/// documents: 74 ms against 12 ms per query).
/// <code>
/// var spec = TextSearch.For&lt;BuyDocument&gt;()
///     .Column(b =&gt; b.TransRefCode)
///     .Lookup(context.Transactors, t =&gt; t.Id, b =&gt; b.TransactorId, t =&gt; t.Name);
/// query = await spec.ApplyAsync(query, searchText);
/// </code>
/// </summary>
public sealed class TextSearchSpec<T>
{
    private readonly List<Expression<Func<T, string>>> _columns = new();
    private readonly List<ILookupTable> _lookups = new();

    /// <summary>A text column of the main table itself.</summary>
    public TextSearchSpec<T> Column(Expression<Func<T, string>> column)
    {
        _columns.Add(column);
        return this;
    }

    /// <summary>
    /// Text columns of a lookup table, linked to the main table by <paramref name="foreignKey"/>
    /// = <paramref name="id"/>.
    /// </summary>
    public TextSearchSpec<T> Lookup<TEntity, TKey>(IQueryable<TEntity> source,
        Expression<Func<TEntity, TKey>> id, Expression<Func<T, TKey>> foreignKey,
        params Expression<Func<TEntity, string>>[] columns)
    {
        _lookups.Add(new LookupTable<TEntity, TKey>(source, id, foreignKey, columns));
        return this;
    }

    /// <summary>
    /// Filters the query to the rows where every search word is found in one of the columns.
    /// Runs one small query per word and lookup table first. No words leaves the query as it is.
    /// </summary>
    public async Task<IQueryable<T>> ApplyAsync(IQueryable<T> query, string searchText)
    {
        var words = TextSearch.GetSearchWords(searchText);
        if (words.Count == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "p");
        var collatedColumns = TextSearch.CollateColumns(_columns, parameter);

        Expression allWords = null;
        foreach (var variants in words)
        {
            var anyColumn = TextSearch.MatchAnyColumn(collatedColumns, variants);
            foreach (var lookup in _lookups)
            {
                var match = await lookup.MatchAsync(parameter, variants);
                if (match != null)
                {
                    anyColumn = anyColumn == null ? match : Expression.OrElse(anyColumn, match);
                }
            }
            // A word found nowhere: no row can match.
            anyColumn ??= Expression.Constant(false);
            allWords = allWords == null ? anyColumn : Expression.AndAlso(allWords, anyColumn);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(allWords!, parameter));
    }

    private interface ILookupTable
    {
        /// <summary>foreignKey IN (ids of the lookup rows that contain the word); null when none do.</summary>
        Task<Expression> MatchAsync(ParameterExpression parameter, string[] variants);
    }

    private sealed class LookupTable<TEntity, TKey> : ILookupTable
    {
        private readonly IQueryable<TEntity> _source;
        private readonly Expression<Func<TEntity, TKey>> _id;
        private readonly Expression<Func<T, TKey>> _foreignKey;
        private readonly Expression<Func<TEntity, string>>[] _columns;

        public LookupTable(IQueryable<TEntity> source, Expression<Func<TEntity, TKey>> id,
            Expression<Func<T, TKey>> foreignKey, Expression<Func<TEntity, string>>[] columns)
        {
            _source = source;
            _id = id;
            _foreignKey = foreignKey;
            _columns = columns;
        }

        public async Task<Expression> MatchAsync(ParameterExpression parameter, string[] variants)
        {
            var entity = Expression.Parameter(typeof(TEntity), "e");
            var match = TextSearch.MatchAnyColumn(TextSearch.CollateColumns(_columns, entity), variants);
            if (match == null)
            {
                return null;
            }

            var ids = await _source
                .Where(Expression.Lambda<Func<TEntity, bool>>(match, entity))
                .Select(_id)
                .Distinct()
                .ToListAsync();
            if (ids.Count == 0)
            {
                return null;
            }

            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(TKey) },
                TextSearch.AsParameter(ids), TextSearch.ReplaceParameter(_foreignKey, parameter));
        }
    }
}
