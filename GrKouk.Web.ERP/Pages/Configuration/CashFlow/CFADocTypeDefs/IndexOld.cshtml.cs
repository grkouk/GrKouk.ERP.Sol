using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.Configuration.CashFlow.CFADocTypeDefs
{
    [Authorize(Roles = "Admin")]
    public class IndexModelOld : PageModel
    {
      
        public IndexModelOld(ApiDbContext context, IMapper mapper)
        {
           
        }
      
        public void OnGet()
        {
           
           
        }

       
    }
}
