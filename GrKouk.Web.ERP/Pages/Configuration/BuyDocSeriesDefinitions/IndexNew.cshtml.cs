using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocSeriesDefinitions
{
    [Authorize(Roles = "Admin,Operator")]
    public class IndexNewModel : PageModel
    {
        public IndexNewModel(ApiDbContext context, IMapper mapper)
        {
            // Minimal PageModel to match CashFlowAccounts
        }

        public void OnGet()
        {
            // Client-side data loading via JS
        }
    }
}