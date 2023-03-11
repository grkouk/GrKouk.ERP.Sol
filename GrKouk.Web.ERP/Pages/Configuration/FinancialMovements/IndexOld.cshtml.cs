using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.FinancialMovements
{
    public class IndexModelOld : PageModel
    {
        private readonly ApiDbContext _context;

        public IndexModelOld(ApiDbContext context)
        {
            _context = context;
        }

        public IList<FinancialMovement> FinancialMovement { get; set; }

        public async Task OnGetAsync()
        {
            FinancialMovement = await _context.FinancialMovements.ToListAsync();
        }
    }
}
