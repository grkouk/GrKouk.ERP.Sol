using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.Configuration.BuyDocSeriesDefinitions
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification toastNotification;

        public EditModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        [BindProperty]
        public BuyDocSeriesDef BuyDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BuyDocSeriesDef = await _context.BuyDocSeriesDefs
                .Include(b => b.BuyDocTypeDef)
                .Include(b => b.Company).FirstOrDefaultAsync(m => m.Id == id);

            if (BuyDocSeriesDef == null)
            {
                return NotFound();
            }
            LoadCombos();
            return Page();
        }

        private void LoadCombos()
        {
            var seriesAutoPayOffList = Enum.GetValues(typeof(SeriesAutoPayoffEnum))
                .Cast<SeriesAutoPayoffEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();
            ViewData["AutoPayoffWay"] = new SelectList(seriesAutoPayOffList, "Value", "Text");
            ViewData["BuyDocTypeDefId"] = new SelectList(_context.BuyDocTypeDefs.OrderBy(p=>p.Name).AsNoTracking(), "Id", "Name");
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["PayoffSeriesId"] = new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(BuyDocSeriesDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("Saved");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyMaterialDocSeriesDefExists(BuyDocSeriesDef.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool BuyMaterialDocSeriesDefExists(int id)
        {
            return _context.BuyDocSeriesDefs.Any(e => e.Id == id);
        }
    }
}
