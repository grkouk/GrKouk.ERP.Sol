using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
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
