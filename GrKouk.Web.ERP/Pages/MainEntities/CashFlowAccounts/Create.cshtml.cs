using System;
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
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.CashFlowAccounts
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;

        public CreateModel(ApiDbContext context, IToastNotification toastNotification, IMapper mapper)
        {
            _context = context;
            _toastNotification = toastNotification;
            _mapper = mapper;
        }

        public IActionResult OnGet()
        {
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


            ViewData["CompaniesListJs"] = companiesListJs;
        }
        [BindProperty]
        public CashFlowAccountCreateDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var itemToAttach = _mapper.Map<CashFlowAccount>(ItemVm);
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                itemToAttach.DateCreated = DateTime.Today;
                itemToAttach.DateLastModified = DateTime.Today;
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
                await _context.CashFlowAccounts.AddAsync(itemToAttach);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _toastNotification.AddSuccessToastMessage("CashFlow Account Created");

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
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