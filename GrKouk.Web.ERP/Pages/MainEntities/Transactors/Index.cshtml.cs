using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
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
