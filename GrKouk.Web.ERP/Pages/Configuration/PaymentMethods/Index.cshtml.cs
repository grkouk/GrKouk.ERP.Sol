using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModel(ApiDbContext context)
        {
            _context = context;
        }

        public IList<PaymentMethod> PaymentMethod { get;set; }

        public async Task OnGetAsync()
        {
            PaymentMethod = await _context.PaymentMethods.ToListAsync();
        }
    }
}
