﻿using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Dtos.CashFlowAccounts;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.CashFlowAccounts
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public CashFlowAccountModifyDto ItemVm { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemToModify = await _context.CashFlowAccounts

                .Include(p => p.CompanyMappings)
                .FirstOrDefaultAsync(m => m.Id == id); ;


            if (itemToModify == null)
            {
                return NotFound();
            }

            ItemVm = _mapper.Map<CashFlowAccountModifyDto>(itemToModify);
            string[] selectedCompanies = itemToModify.CompanyMappings.Select(x => x.CompanyId.ToString()).ToArray();
            ItemVm.SelectedCompanies = JsonSerializer.Serialize(selectedCompanies);
            LoadCombos();
            // _toastNotification.AddInfoToastMessage("Welcome to edit page");
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
            ViewData["CompaniesListJs"] = companiesListJs;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddAlertToastMessage("Please see errors");
                return Page();
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var itemToAttach = _mapper.Map<CashFlowAccount>(ItemVm);
                itemToAttach.DateLastModified = DateTime.Today;
                _context.Attach(itemToAttach).State = EntityState.Modified;
                _context.CashFlowAccountCompanyMappings.RemoveRange(_context.CashFlowAccountCompanyMappings.Where(p => p.CashFlowAccountId == itemToAttach.Id));
                string[] companiesSelected = JsonSerializer.Deserialize<string[]>(ItemVm.SelectedCompanies);

                foreach (var i in companiesSelected)
                {
                    if (!Int32.TryParse(i, out int compId))
                    {
                        throw new Exception("Selected company Id error");
                    }
                    itemToAttach.CompanyMappings.Add(new CashFlowAccountCompanyMapping()
                    {
                        CompanyId = compId,
                        CashFlowAccountId = itemToAttach.Id
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _toastNotification.AddSuccessToastMessage("Cash Flow Account changes saved");
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Concurrency error");
                if (!ItemExists(ItemVm.Id))
                {
                    _toastNotification.AddErrorToastMessage("Cash flow Account was not found");
                    return NotFound();
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Concurrency error");
                    LoadCombos();
                    return Page();
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", e.Message);
                _toastNotification.AddErrorToastMessage(e.Message);
                LoadCombos();
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private bool ItemExists(int id)
        {
            return _context.CashFlowAccounts.Any(e => e.Id == id);
        }
    }
}
