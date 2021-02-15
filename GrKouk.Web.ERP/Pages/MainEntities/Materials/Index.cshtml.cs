using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {

        public IndexModel()
        {
           
        }


        public void OnGet()
        {
           
            
           
        }
       
    }
}
