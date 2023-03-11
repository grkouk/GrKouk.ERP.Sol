using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.Configuration.FinancialMovements
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
