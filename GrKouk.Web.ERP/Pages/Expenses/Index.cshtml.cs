﻿using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.Erp.Pages.Expenses
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public IndexModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void OnGet()
        {
            LoadFilters();
        }
        private void LoadFilters()
        {
            var datePeriods = DateFilter.GetDateFiltersSelectList();
           // var datePeriodsJs = DateFilter.GetDateFiltersSelectList();

            ViewData["DataFilterValues"] = new SelectList(datePeriods, "Value", "Text");
            var pageFilterSize = PageFilter.GetPageSizeFiltersSelectList();
            ViewData["PageFilterSize"] = new SelectList(pageFilterSize, "Value", "Text");
            //var dbCompanies = _context.Companies.OrderBy(p => p.Code).AsNoTracking();
            //List<SelectListItem> companiesList = new List<SelectListItem>();
            //companiesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            //foreach (var company in dbCompanies)
            //{
            //    companiesList.Add(new SelectListItem() { Value = company.Id.ToString(), Text = company.Code });
            //}
            var companiesList = FiltersHelper.GetCompaniesFilterList(_context);
            ViewData["CompanyFilter"] = new SelectList(companiesList, "Value", "Text");


        }
    }
}