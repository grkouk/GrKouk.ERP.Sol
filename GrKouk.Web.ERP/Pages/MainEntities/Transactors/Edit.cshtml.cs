using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin,Operator")]
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;

        public EditModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public TransactorModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbTransactor = await _context.Transactors
                .Include(t => t.TransactorType)
                .Include(p=>p.TransactorCompanyMappings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dbTransactor == null)
            {
                return NotFound();
            }

            ItemVm = dbTransactor.ToModifyDto();
            string[] selectedCompanies = dbTransactor.TransactorCompanyMappings.Select(x => x.CompanyId.ToString()).ToArray();
            ItemVm.SelectedCompanies = JsonSerializer.Serialize(selectedCompanies);

           
            LoadCombos();

            return Page();
        }

        private void LoadCombos()
        {
            var companiesListJs = _context.Companies.OrderBy(p => p.Name)
                .Select(p => new UISelectTypeItem()
                {
                    Title = p.Name,
                    ValueInt = p.Id,
                    Value = p.Id.ToString()
                }).ToList();
            ViewData["TransactorTypeId"] = new SelectList(_context.TransactorTypes.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            //ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["CompaniesListJs"] = companiesListJs;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            // Load the stored row and copy only the editable fields onto it, so columns the
            // form does not carry (e.g. DateCreated) keep their values.
            var transactorToAdd = await _context.Transactors.FirstOrDefaultAsync(t => t.Id == ItemVm.Id);
            if (transactorToAdd == null)
            {
                return NotFound();
            }
            // Optimistic concurrency: compare against the row version the form was loaded with.
            _context.Entry(transactorToAdd).Property(t => t.Timestamp).OriginalValue = ItemVm.Timestamp;
            ItemVm.ApplyTo(transactorToAdd);
            transactorToAdd.DateLastModified=DateTime.Today;
            
            _context.TransactorCompanyMappings.RemoveRange(_context.TransactorCompanyMappings.Where(p => p.TransactorId == transactorToAdd.Id));
            
            string[] companiesSelected = JsonSerializer.Deserialize<string[]>(ItemVm.SelectedCompanies);
            foreach (var i in companiesSelected)
            {
                if (!Int32.TryParse(i, out int compId))
                {
                    throw new Exception("Selected company Id error");
                }
                
                transactorToAdd.TransactorCompanyMappings.Add(new TransactorCompanyMapping()
                {
                    CompanyId = compId,
                    TransactorId = transactorToAdd.Id
                });
            }
            try
            {
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Modifications saved!");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactorExists(transactorToAdd.Id))
                {
                    return NotFound();
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Not saved concurrency exception.");
                }
            }
            catch (Exception e)
            {
                _toastNotification.AddErrorToastMessage(e.Message);
            }

            return RedirectToPage("./Index");
        }

        private bool TransactorExists(int id)
        {
            return _context.Transactors.Any(e => e.Id == id);
        }
    }
}
