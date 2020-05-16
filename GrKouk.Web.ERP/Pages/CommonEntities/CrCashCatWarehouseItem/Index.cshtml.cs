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

namespace GrKouk.Web.Erp.Pages.CommonEntities.CrCashCatWarehouseItem
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;


        public IndexModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
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
            var materialNatures = FiltersHelper.GetWarehouseItemNaturesList();

            ViewData["MaterialNatureValues"] = new SelectList(materialNatures, "Value", "Text");
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

            var dbClientProfiles = _context.ClientProfiles.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> clientProfilesList = new List<SelectListItem>();
            clientProfilesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Profiles}" });
            foreach (var clientProfile in dbClientProfiles)
            {
                clientProfilesList.Add(new SelectListItem() { Value = clientProfile.Id.ToString(), Text = clientProfile.Name });
            }
            ViewData["ClientProfileFilter"] = new SelectList(clientProfilesList, "Value", "Text");

            var dbCashRegCategories = _context.CashRegCategories.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> cashRegRegCateroryList = new List<SelectListItem>();
            cashRegRegCateroryList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Categories}" });
            foreach (var cashRegCategory in dbCashRegCategories)
            {
                cashRegRegCateroryList.Add(new SelectListItem() { Value = cashRegCategory.Id.ToString(), Text = cashRegCategory.Name });
            }
            ViewData["CashRegCategoryFilter"] = new SelectList(cashRegRegCateroryList, "Value", "Text");
        }
    }
}
