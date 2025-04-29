using System.Linq;
using AutoMapper;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.Sync;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Sync.Logging
{
    [Authorize(Roles = "Admin,Operator")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        
        public IndexModel(ApiDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #region Fields
        public string SectionCode { get; set; } = "SYS-CFA-TRANS";
        #endregion
        public decimal sumCredit = 0;
        public decimal sumDebit = 0;
        public PagedList<SynchronizationLogListDto> ListItems { get; set; }
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
