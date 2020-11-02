using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;

        public EditModel(ApiDbContext context, IToastNotification toastNotification,IMapper mapper)
        {
            _context = context;
            _toastNotification = toastNotification;
            _mapper = mapper;
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

            ItemVm = _mapper.Map<TransactorModifyDto>(dbTransactor);
            string cmps="";
            bool isFirst = true;
            foreach (var companyMapping in dbTransactor.TransactorCompanyMappings)
            {
                if (!isFirst)
                {
                    cmps += ",";
                }
                else
                {
                    isFirst = false;
                }

                cmps += companyMapping.CompanyId.ToString();
            }

            ItemVm.SelectedCompanies = cmps;    
            LoadCombos();

            return Page();
        }

        private void LoadCombos()
        {
            var companiesListJs = _context.Companies.OrderBy(p => p.Name)
                .Select(p => new DiaryDocTypeItem()
                {
                    Title = p.Name,
                    Value = p.Id
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
            var transactorToAdd = _mapper.Map<Transactor>(ItemVm);
            transactorToAdd.DateLastModified=DateTime.Today;
            _context.Attach(transactorToAdd).State = EntityState.Modified;
            //transactorToAdd.TransactorCompanyMappings.Clear();
            _context.TransactorCompanyMappings.RemoveRange(_context.TransactorCompanyMappings.Where(p => p.TransactorId == transactorToAdd.Id));
            
            if (!String.IsNullOrEmpty(ItemVm.SelectedCompanies))
            {
                var listOfCompanies = ItemVm.SelectedCompanies.Split(",");
                //bool fl = true;
                foreach (var listOfCompany in listOfCompanies)
                {
                    int.TryParse(listOfCompany, out var companyId);
                    if (companyId > 0)
                    {
                        transactorToAdd.TransactorCompanyMappings.Add(new TransactorCompanyMapping
                        {
                            CompanyId = companyId,
                            TransactorId = transactorToAdd.Id
                        });
                        //TODO: remove when companyid column removed for transactor entity
                        //if (fl)
                        //{
                        //    transactorToAdd.CompanyId = companyId;
                        //    fl = false;
                        //}

                    }
                }

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
