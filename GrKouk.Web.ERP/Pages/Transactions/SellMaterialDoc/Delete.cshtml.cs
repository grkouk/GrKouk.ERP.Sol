using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SellDocument SaleDocument { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SaleDocument = await _context.SellDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.SellDocSeries)
                .Include(b => b.SellDocType)
                .Include(b => b.Section)
                .Include(b => b.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (SaleDocument == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // const string sectionCode = "SYS-SELL-COMBINED-SCN";
            if (id == null)
            {
                return NotFound();
            }
           
            SaleDocument = await _context.SellDocuments.FindAsync(id);

            if (SaleDocument != null)
            {
                _context.SellDocLines.RemoveRange(_context.SellDocLines.Where(p => p.SellDocumentId == id));
                _context.TransactorTransactions.RemoveRange(_context.TransactorTransactions.Where(p => p.CreatorSectionId == SaleDocument.SectionId && p.CreatorId == id));
                _context.WarehouseTransactions.RemoveRange(_context.WarehouseTransactions.Where(p => p.SectionId == SaleDocument.SectionId && p.CreatorId == id));
                _context.SellDocTransPaymentMappings.RemoveRange(_context.SellDocTransPaymentMappings.Where(p=>p.SellDocumentId==id));
                _context.SellDocuments.Remove(SaleDocument);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
