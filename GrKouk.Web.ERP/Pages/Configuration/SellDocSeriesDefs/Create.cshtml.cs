using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.SellDocSeriesDefs
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;

        public CreateModel(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
       LoadCombos();
            return Page();
        }

        [BindProperty]
        public SellDocSeriesDef SellDocSeriesDef { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.SellDocSeriesDefs.Add(SellDocSeriesDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
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