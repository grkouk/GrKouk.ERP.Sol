﻿using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.TransactorTransDocTypes
{
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;

        public EditModel(ApiDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransTransactorDocTypeDef TransTransactorDocTypeDef { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TransTransactorDocTypeDef = await _context.TransTransactorDocTypeDefs
                .Include(t => t.Company)
                .Include(t => t.TransTransactorDef).FirstOrDefaultAsync(m => m.Id == id);

            if (TransTransactorDocTypeDef == null)
            {
                return NotFound();
            }
           ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Code");
           ViewData["TransTransactorDefId"] = new SelectList(_context.TransTransactorDefs, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(TransTransactorDocTypeDef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransTransactorDocTypeDefExists(TransTransactorDocTypeDef.Id))
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

        private bool TransTransactorDocTypeDefExists(int id)
        {
            return _context.TransTransactorDocTypeDefs.Any(e => e.Id == id);
        }
    }
}
