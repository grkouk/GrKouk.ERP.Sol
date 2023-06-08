using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class IsozigioModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public IsozigioModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
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

            var companiesList = FiltersHelper.GetCompaniesFilterList(_context);
            ViewData["CompanyFilter"] = new SelectList(companiesList, "Value", "Text");
            ViewData["CurrencySelector"] = new SelectList(FiltersHelper.GetCurrenciesFilterList(_context), "Value", "Text");
            var currencyListJs = _context.Currencies.OrderBy(p => p.Id).AsNoTracking().ToList();
            ViewData["CurrencyListJs"] = currencyListJs;
        }
    }
}