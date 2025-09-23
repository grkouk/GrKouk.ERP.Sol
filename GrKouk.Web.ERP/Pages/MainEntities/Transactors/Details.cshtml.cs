using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin,Operator")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private int _id;


        public DetailsModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty] public TransactorDetailDto Item { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadFiltersAsync();
            if (id == null)
            {
                return NotFound();
            }

            _id = (int)id;
            ViewData["TransactorId"] = id;
            var transactor = await _context.Transactors
                .Include(t => t.TransactorCompanyMappings)
                .ThenInclude(x => x.Company)
                .Include(t => t.TransactorType).FirstOrDefaultAsync(m => m.Id == id);

            if (transactor == null)
            {
                return NotFound();
            }

            Item = _mapper.Map<TransactorDetailDto>(transactor);
            var compList = transactor.TransactorCompanyMappings.Select(x => x.Company.Code).ToList();
            Item.Companies = String.Join(",", compList);
            var transactorTitle = $"{Item.TransactorTypeName} {Item.Name}";
            ViewData["ItemTitle"] = transactorTitle;
            ViewData["Title"] = $"{transactorTitle}-Details";
            return Page();
        }

        private async Task LoadFiltersAsync()
        {
            Func<Task<List<UISelectTypeItem>>> companiesListJsFunc = async () =>
            {
                var itemsList = await _context.Companies
                    .OrderBy(p => p.Code)
                    .Select(p => new UISelectTypeItem()
                    {
                        Title = p.Code,
                        ValueInt = p.Id,
                        Value = p.Id.ToString()
                    })
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<Currency>>> currenciesListJsFunc = async () =>
            {
                var itemsList = await _context.Currencies
                    .OrderBy(p => p.Name)
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };

            ViewData["CurrencyListJs"] = await currenciesListJsFunc();
            ViewData["DatePeriodListJs"] = DateFilter.GetDateFiltersSelectList();
            ViewData["CompaniesListJs"] = await companiesListJsFunc();
            ViewData["AllCompaniesId"] = await FiltersHelper.GetAllCompaniesIdAsync(_context);
        }
    }
}