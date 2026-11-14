using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors {
    [Authorize(Roles = "Admin,Operator")]
    public class CreateModel : PageModel {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;

        public CreateModel(ApiDbContext context, IToastNotification toastNotification) {
            _context = context;
            _toastNotification = toastNotification;
        }

        public IActionResult OnGet() {
            LoadCombos();
            return Page();
        }
        private void LoadCombos() {
            var companiesListJs = _context.Companies.OrderBy(p => p.Name)
                .Select(p => new UISelectTypeItem() {
                    Title = p.Name,
                    ValueInt = p.Id,
                    Value = p.Id.ToString()
                }).ToList();
            ViewData["TransactorTypeId"] = new SelectList(_context.TransactorTypes.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            //ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["CompaniesListJs"] = companiesListJs;
        }
        [BindProperty]
        public TransactorCreateDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }
            var itemToAttach = ItemVm.ToEntity();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                itemToAttach.DateCreated = DateTime.Today;
                itemToAttach.DateLastModified = DateTime.Today;
                string[] companiesSelected = JsonSerializer.Deserialize<string[]>(ItemVm.SelectedCompanies);
                if (companiesSelected != null) {
                    foreach (var i in companiesSelected) {
                        if (!Int32.TryParse(i, out int compId)) {
                            throw new Exception("Selected company Id error");
                        }

                        itemToAttach.TransactorCompanyMappings.Add(new TransactorCompanyMapping() {
                            CompanyId = compId,
                            TransactorId = itemToAttach.Id
                        });
                    }
                }

                await _context.Transactors.AddAsync(itemToAttach);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _toastNotification.AddSuccessToastMessage("Transactor Created");

                return RedirectToPage("./Index");
            }
            catch (Exception e) {
                Console.WriteLine(e);
                await transaction.RollbackAsync();
                ModelState.AddModelError("", e.Message);
                _toastNotification.AddErrorToastMessage(e.Message);
                LoadCombos();
                return Page();
            }

        }
    }
}