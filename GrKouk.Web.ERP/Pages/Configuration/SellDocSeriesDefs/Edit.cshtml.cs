using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocSeriesDefs
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SellDocSeriesDef SellDocSeriesDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SellDocSeriesDef = await _context.SellDocSeriesDefs
                .Include(s => s.Company)
                .Include(s => s.SellDocTypeDef).FirstOrDefaultAsync(m => m.Id == id);

            if (SellDocSeriesDef == null)
            {
                return NotFound();
            }
           LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(SellDocSeriesDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SellDocSeriesDefExists(SellDocSeriesDef.Id))
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

        private bool SellDocSeriesDefExists(int id)
        {
            return _context.SellDocSeriesDefs.Any(e => e.Id == id);
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
            ViewData["SellDocTypeDefId"] = new SelectList(_context.SellDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["PayoffSeriesId"] = new SelectList(_context.TransTransactorDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
        }
    }
}
