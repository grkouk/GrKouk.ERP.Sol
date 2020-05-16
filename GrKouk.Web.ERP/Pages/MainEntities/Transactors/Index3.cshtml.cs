using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class Index3Model : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public Index3Model(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public string NameSort { get; set; }
        public string NameSortIcon { get; set; }
        public string DateSort { get; set; }
        public string DateSortIcon { get; set; }

        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public int PageSize { get; set; }
        public int CurrentTransactorTypeFilter { get; set; }
        public List<SearchListItem> RelevantDiarys { get; set; }


        //public int CurrentPageSize { get; set; }
        public PagedList<TransactorListDto> ListItems { get; set; }
        public async Task OnGetAsync(string sortOrder, string searchString, int? transactorTypeFilter, int? pageIndex, int? pageSize)
        {
            LoadFilters();

            PageSize = (int)((pageSize == null || pageSize == 0) ? 20 : pageSize);
            // CurrentPageSize = PageSize;
            CurrentSort = sortOrder;
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = CurrentFilter;
            }
            CurrentFilter = searchString;
            if (transactorTypeFilter == null)
            {
                transactorTypeFilter = 0;
            }
            CurrentTransactorTypeFilter = (int)(transactorTypeFilter ?? 0);
            await LoadDiarysAsync();

            IQueryable<Transactor> fullListIq = from s in _context.Transactors
                                                select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                fullListIq = fullListIq.Where(s => s.Name.Contains(searchString));
            }

            if (transactorTypeFilter > 0)
            {
                fullListIq = fullListIq.Where(p => p.TransactorTypeId == transactorTypeFilter);
            }
            switch (sortOrder)
            {

                case "Name":
                    fullListIq = fullListIq.OrderBy(p => p.Name);
                    NameSortIcon = "fas fa-sort-alpha-up ";
                    DateSortIcon = "invisible";
                    break;
                case "name_desc":
                    fullListIq = fullListIq.OrderByDescending(p => p.Name);
                    NameSortIcon = "fas fa-sort-alpha-down ";
                    DateSortIcon = "invisible";
                    break;
                default:
                    fullListIq = fullListIq.OrderBy(p => p.Id);
                    break;
            }

            var t = fullListIq.ProjectTo<TransactorListDto>(_mapper.ConfigurationProvider);


            ListItems = await PagedList<TransactorListDto>.CreateAsync(
                t, pageIndex ?? 1, PageSize);

        }

        private async Task LoadDiarysAsync()
        {
            RelevantDiarys=new List<SearchListItem>();

            

            var dList = await _context.DiaryDefs.Where(p => p.DiaryType == DiaryTypeEnum.DiaryTypeEnumTransactors)
                .ToListAsync();
            if (CurrentTransactorTypeFilter != 0)
            {
                var tf = CurrentTransactorTypeFilter;
                //var transTypes = Array.ConvertAll(diaryDef.SelectedTransTypes.Split(","), int.Parse);
                RelevantDiarys = dList
                    .Where(t => Array.ConvertAll(t.SelectedTransTypes.Split(","), int.Parse).Contains(tf))
                        .Select(item => new SearchListItem()
                        {
                            Value = item.Id,
                            Text = item.Name
                        })
                        .ToList();

            }
        }
        private void LoadFilters()
        {
            var dbTransactorTypes = _context.TransactorTypes.OrderBy(p => p.Code).AsNoTracking();
            List<SelectListItem> transactorTypes = new List<SelectListItem>();
            transactorTypes.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Types}" });
            foreach (var dbTransactorType in dbTransactorTypes)
            {
                transactorTypes.Add(new SelectListItem() { Value = dbTransactorType.Id.ToString(), Text = dbTransactorType.Code });
            }
            ViewData["TransactorTypeId"] = new SelectList(transactorTypes, "Value", "Text");
            var pageFilterSize = PageFilter.GetPageSizeFiltersSelectList();
            ViewData["PageFilterSize"] = new SelectList(pageFilterSize, "Value", "Text");
        }
    }
}
