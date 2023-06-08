using AutoMapper;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.Configuration.CashFlow.CFADocTypeDefs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context) => _context = context;

        public void OnGet()
        {
           
           
        }

       
    }
}
