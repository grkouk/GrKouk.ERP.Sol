using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.BuyMaterialsDoc
{
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;

        public DeleteModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BuyDocument BuyDocument { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocument = await _context.BuyDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.BuyDocSeries)
                .Include(b => b.BuyDocType)
                .Include(b => b.Section)
                .Include(b => b.Transactor).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocument == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            if (id == null)
            {
                return NotFound();
            }
           
            BuyDocument = await _context.BuyDocuments.FindAsync(id);

            if (BuyDocument != null)
            {

                _context.BuyDocLines.RemoveRange(_context.BuyDocLines.Where(p => p.BuyDocumentId == id));
                _context.TransactorTransactions.RemoveRange(_context.TransactorTransactions.Where(p => p.CreatorSectionId == BuyDocument.SectionId && p.CreatorId == id));
                _context.WarehouseTransactions.RemoveRange(_context.WarehouseTransactions.Where(p => p.SectionId == BuyDocument.SectionId && p.CreatorId == id));
                _context.BuyDocTransPaymentMappings.RemoveRange(_context.BuyDocTransPaymentMappings.Where(p=>p.BuyDocumentId==id));
                _context.BuyDocuments.Remove(BuyDocument);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
