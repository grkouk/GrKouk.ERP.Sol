using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.Configuration.CashFlow.CFATransactionDefs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
      
        public IndexModel(ApiDbContext context)
        {
           
        }
      
        public void OnGet()
        {
           
           
        }

       
    }
}
