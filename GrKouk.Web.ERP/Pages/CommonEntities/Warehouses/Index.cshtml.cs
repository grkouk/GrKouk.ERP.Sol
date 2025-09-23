using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.CommonEntities.Warehouses
{
    [Authorize(Roles = "Admin,Operator")]
    public class IndexModel : PageModel
    {
      
        public IndexModel(ApiDbContext context, IMapper mapper)
        {
           
        }
      
        public void OnGet()
        {
           
           
        }

       
    }
}
