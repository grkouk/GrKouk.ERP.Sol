using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Helpers.SfGrid;

/// <summary>
/// The sortable columns of an index grid: grid field name -> sort key. Single-column sort,
/// as on the other index pages; a tie breaker (usually Id) keeps paging stable.
/// <code>
/// static readonly SfSortMap&lt;Dto&gt; Sorts = new SfSortMap&lt;Dto&gt;(p =&gt; p.Id)
///     .Add("transDate", p =&gt; p.TransDate, isDefault: true)
///     .Add("transactorName", p =&gt; p.TransactorName);
/// </code>
/// </summary>
public class SfSortMap<T>
{
    private readonly Dictionary<string, Func<IQueryable<T>, bool, IOrderedQueryable<T>>> _sorts =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Expression<Func<T, int>> _tieBreaker;
    private Func<IQueryable<T>, bool, IOrderedQueryable<T>> _default;

    public SfSortMap(Expression<Func<T, int>> tieBreaker)
    {
        _tieBreaker = tieBreaker;
    }

    public SfSortMap<T> Add<TKey>(string field, Expression<Func<T, TKey>> key, bool isDefault = false)
    {
        IOrderedQueryable<T> Sort(IQueryable<T> query, bool desc) =>
            desc ? query.OrderByDescending(key) : query.OrderBy(key);

        _sorts[field] = Sort;
        if (isDefault || _default == null)
        {
            _default = Sort;
        }
        return this;
    }

    /// <summary>Sorts by the grid's first sort column; unknown or no column -> the default key.</summary>
    public IQueryable<T> Apply(IQueryable<T> query, List<Sort> sorted)
    {
        var sort = sorted?.FirstOrDefault();
        var desc = string.Equals(sort?.Direction, "descending", StringComparison.OrdinalIgnoreCase);
        var apply = sort?.Name != null && _sorts.TryGetValue(sort.Name, out var found) ? found : _default;
        if (apply == null)
        {
            return query.OrderBy(_tieBreaker);
        }
        return apply(query, desc).ThenBy(_tieBreaker);
    }
}
