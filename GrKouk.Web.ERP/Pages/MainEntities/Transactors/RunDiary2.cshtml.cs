using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class RunDiaryModel2 : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public RunDiaryModel2(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public PagedList<TransactorTransListDto> ListItems { get; set; }
        public int PageSizeKartela { get; set; }
        public string TransactorName { get; set; }
        public int TransactorId { get; set; }

        public int ParentPageSize { get; set; }
        public int ParentPageIndex { get; set; }
        public int TransactorTypeFilter { get; set; }
        public int CurrentTransactorTypeFilter { get; set; }
        public int CompanyFilter { get; set; }
        public string IsozigioName { get; set; }
        public string NameSort { get; set; }
        public string NameSortIcon { get; set; }
        public string DateSort { get; set; }
        public string DateSortIcon { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentDatePeriod { get; set; }
        public int DiaryId { get; set; }
        public decimal sumCredit = 0;
        public decimal sumDebit = 0;
        public async Task<IActionResult> OnGetAsync(int? diaryId, string sortOrder, string searchString, string datePeriodFilter
            , int? pageIndexKartela, int? pageSizeKartela,int? transactorTypeFilter, int? companyFilter, int? parentPageIndex
            , int? parentPageSize)
        {
            if (diaryId == null)
            {
                return NotFound();
            }

            
            var diaryDef = await _context.DiaryDefs.FindAsync(diaryId);
            if (diaryDef == null)
            {
                return NotFound();
            }
            LoadFilters();
            DiaryId = (int)(diaryId ?? 0);
            CompanyFilter = (int)(companyFilter ?? 0);
            ParentPageSize = (int)(parentPageSize ?? 0);
            ParentPageIndex = (int)(parentPageIndex ?? 0);
            PageSizeKartela = (int)((pageSizeKartela == null || pageSizeKartela == 0) ? 20 : pageSizeKartela);
            TransactorTypeFilter = (int)(transactorTypeFilter ?? 0);

            CurrentSort = sortOrder;
            NameSort = sortOrder == "Name" ? "name_desc" : "Name";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageIndexKartela = 1;
            }
            else
            {
                searchString = CurrentFilter;
            }
            CurrentFilter = searchString;
            CurrentDatePeriod = datePeriodFilter;

            IQueryable<TransactorTransaction> fullListIq = _context.TransactorTransactions;
            if (diaryDef.SelectedTransTypes!=null)
            {
                var transTypes = Array.ConvertAll(diaryDef.SelectedTransTypes.Split(","), int.Parse);
                fullListIq = fullListIq.Where(p => transTypes.Contains(p.Transactor.TransactorTypeId));
            }
            if (diaryDef.SelectedDocTypes != null)
            {
                var docTypes = Array.ConvertAll(diaryDef.SelectedDocTypes.Split(","), int.Parse);
                fullListIq = fullListIq.Where(p => docTypes.Contains(p.TransTransactorDocTypeId));
            }

            DateFilterDates dfDates = DateFilter.GetDateFilterDates(datePeriodFilter);
            DateTime fromDate = dfDates.FromDate;
            DateTime toDate = dfDates.ToDate;
            CurrentDatePeriod = dfDates.FilterName;
           
            fullListIq = fullListIq.Where(p => p.TransDate >= fromDate && p.TransDate <= toDate);
            if (companyFilter > 0)
            {
                fullListIq = fullListIq.Where(p => p.CompanyId == companyFilter);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                fullListIq = fullListIq.Where(s => s.Transactor.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Date":
                    fullListIq = fullListIq.OrderBy(p => p.TransDate);
                    DateSortIcon = "fas fa-sort-numeric-up ";
                    NameSortIcon = "invisible";
                    break;
                case "date_desc":
                    fullListIq = fullListIq.OrderByDescending(p => p.TransDate);
                    DateSortIcon = "fas fa-sort-numeric-down ";
                    NameSortIcon = "invisible";
                    break;
                case "Name":
                    fullListIq = fullListIq.OrderBy(p => p.Transactor.Name);
                    NameSortIcon = "fas fa-sort-alpha-up ";
                    DateSortIcon = "invisible";
                    break;
                case "name_desc":
                    fullListIq = fullListIq.OrderByDescending(p => p.Transactor.Name);
                    NameSortIcon = "fas fa-sort-alpha-down ";
                    DateSortIcon = "invisible";
                    break;
                default:
                    fullListIq = fullListIq.OrderBy(p => p.Id);
                    break;
            }

            var t = fullListIq.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);
            ListItems = await PagedList<TransactorTransListDto>.CreateAsync(
                t, pageIndexKartela ?? 1, PageSizeKartela);

            sumCredit = ListItems.Sum(p => p.CreditAmount);
            sumDebit = ListItems.Sum(p => p.DebitAmount);
            return Page();
        }

        private void LoadFilters()
        {
           
            var datePeriods = DateFilter.GetDateFiltersSelectList();
            ViewData["DataFilterValues"] = new SelectList(datePeriods, "Value", "Text");

            var pageFilterSize = PageFilter.GetPageSizeFiltersSelectList();
            ViewData["PageFilterSize"] = new SelectList(pageFilterSize, "Value", "Text");

            var dbCompanies = _context.Companies.OrderBy(p => p.Code).AsNoTracking();
            List<SelectListItem> companiesList = new List<SelectListItem>();
            companiesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var company in dbCompanies)
            {
                companiesList.Add(new SelectListItem() { Value = company.Id.ToString(), Text = company.Code });
            }
            ViewData["CompanyFilter"] = new SelectList(companiesList, "Value", "Text");
        }
    }
}