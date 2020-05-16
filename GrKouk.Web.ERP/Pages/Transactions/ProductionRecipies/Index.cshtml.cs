using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.ProductionRecipies
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        #region Fields

        public string SectionCode { get; set; } = "SYS-BUY-MATERIALS-SCN";
        #endregion
        public IndexModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }


        public PagedList<BuyDocListDto> ListItems { get; set; }
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
