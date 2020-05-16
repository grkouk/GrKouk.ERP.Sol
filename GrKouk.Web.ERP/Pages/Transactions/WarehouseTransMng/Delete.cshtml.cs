using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.WarehouseTransMng
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;

        private const string SectionSystemCode = "SYS-WAREHOUSE-TRANS";
        public DeleteModel(ApiDbContext context,IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public WarehouseTransaction WarehouseTransaction { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransaction = await _context.WarehouseTransactions
                .Include(w => w.Company)
                .Include(w => w.FiscalPeriod)
                .Include(w => w.WarehouseItem)
                .Include(w => w.Section)
                .Include(w => w.TransWarehouseDocSeries)
                .Include(w => w.TransWarehouseDocType).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseTransaction == null)
            {
                return NotFound();
            }
            var section = _context.Sections.SingleOrDefault(s => s.SystemName == SectionSystemCode);
            if (section is null)
            {
                _toastNotification.AddAlertToastMessage("Supplier Transactions section not found in DB");
                return BadRequest();
            }
            //If section is not our section the canot update disable input controls
            NotUpdatable = WarehouseTransaction.SectionId != section.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransaction = await _context.WarehouseTransactions.FindAsync(id);

            if (WarehouseTransaction != null)
            {
                _context.WarehouseTransactions.Remove(WarehouseTransaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
