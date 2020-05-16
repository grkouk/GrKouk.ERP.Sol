using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious
        {
            get
            {
                return CurrentPage > 1;
            }
        }

        public bool HasNext
        {
            get
            {
                return CurrentPage < TotalPages;
            }
        }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            pageSize = pageSize < 0 ? 1 : pageSize;
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize < 0 ? 1 : pageSize;
            var items = await source.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageIndex, pageSize);
        }
        //public static async Task<PagedList<T>> CreateForDataTableAsync(
        //    IQueryable<T> source, int itemsToSkip, int pageSize)
        //{
        //    List<T> items;
        //    var count = await source.CountAsync();
        //    if (pageSize==-1)
        //    {
        //        items = await source.ToListAsync();
        //    }
        //    else
        //    {
        //         items = await source.Skip(itemsToSkip)
        //            .Take(pageSize).ToListAsync();

        //    }

        //    var pageIndex = (int)Math.Ceiling(itemsToSkip / (double)pageSize);
        //    return new PagedList<T>(items, count, pageIndex, pageSize);
        //}
    }
}
