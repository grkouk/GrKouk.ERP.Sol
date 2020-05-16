using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.MainEntities.MaterialCodes
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DetailsModel(ApiDbContext context)
        {
            _context = context;
        }

        public WarehouseItemCode WarehouseItemCode { get; set; }

        public  IActionResult OnGetAsync(int id)
        {
            return Page();
        }
    }
}
