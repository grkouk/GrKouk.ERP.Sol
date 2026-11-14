using System.Linq;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    [Authorize(Roles = "Admin,Operator")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;
       
        #region Fields
        public  string SectionCode { get; set; } = "SYS-SELL-COMBINED-SCN";
        #endregion
        public IndexModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
       
        public PagedList<SellDocListDto> ListItems { get; set; }
        public void OnGet()
        {
            LoadFilters();

        }
        private void LoadFilters()
        {
            var currencyListJs = _context.Currencies.OrderBy(p => p.Id).AsNoTracking().ToList();
            ViewData["CurrencyListJs"] = currencyListJs;
        }
    }
}
