using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers.SfGrid;

/// <summary>Server-side paging for the Syncfusion index grids (UrlAdaptor).</summary>
public static class SfGrid
{
    /// <summary>
    /// Counts the filtered query, then sorts it and loads the requested page.
    /// </summary>
    public static async Task<(List<T> Rows, int Count)> LoadPageAsync<T>(
        IQueryable<T> query, SfGridRequest dm, SfSortMap<T> sorts)
    {
        var count = await query.CountAsync();

        query = sorts.Apply(query, dm.Sorted);
        if (dm.Skip > 0)
        {
            query = query.Skip(dm.Skip);
        }
        if (dm.Take > 0)
        {
            query = query.Take(dm.Take);
        }

        return (await query.ToListAsync(), count);
    }

    /// <summary>The response the UrlAdaptor expects: rows plus total count when it asked for it.</summary>
    public static JsonResult Result<TRow>(IEnumerable<TRow> rows, int count, SfGridRequest dm) =>
        dm.RequiresCounts
            ? new JsonResult(new { result = rows, count })
            : new JsonResult(rows);
}
