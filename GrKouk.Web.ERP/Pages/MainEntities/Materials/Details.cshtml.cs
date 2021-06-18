using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private int _id;
        public DetailsModel(ApiDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty]
        public WrItemDetailDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _id = (int)id;
            ViewData["ItemId"] = id;
            var item = await _context.WarehouseItems
                .Include(t=>t.CompanyMappings)
                .ThenInclude(x=>x.Company)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }
            //ItemVm=_mapper.Map<WrItemDetailDto>(item);
            int natureCode;
            string natureName;
            (natureCode, natureName) = HelperFunctions.GetWarehouseNatureDetails(item.WarehouseItemNature);
            ItemVm = new WrItemDetailDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                NatureName =natureName ,
                NatureCode = natureCode,
                Companies = null
            };
            
            var compList = item.CompanyMappings.Select(x => x.Company.Code).ToList();
            ItemVm.Companies = String.Join(",", compList);
            var itemTitle = $"{ItemVm.NatureName} {ItemVm.Name}";
            ViewData["ItemTitle"] = itemTitle;
            ViewData["Title"] = $"{itemTitle}-Details";
            await LoadCombosAsync();
            return Page();
        }
         private async Task LoadCombosAsync() {
            Func<Task<List<SelectListItem>>> transactorListFunc = async () => {
                var itemList = await _context.Transactors
                    .Include(p => p.TransactorType)
                    .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                    .OrderBy(s => s.Name)
                    .AsNoTracking()
                    .Select(c => new SelectListItem() {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name}-{c.TransactorType.Code}"
                    })
                    .ToListAsync();
                return itemList;
            };
            Func<Task<List<SelectListItem>>> companiesListFunc = async () => await FiltersHelper.GetCompaniesFilterListAsync(_context);
            Func<Task<List<SelectListItem>>> currenciesListFunc = async () => await FiltersHelper.GetCurrenciesFilterListAsync(_context);
            Func<Task<List<SelectListItem>>> transactorTransDocSeriesListFunc = async () => {
                var itemsList = await _context.TransTransactorDocSeriesDefs
                    .OrderBy(s => s.Name)
                    .AsNoTracking()
                    .Select(c => new SelectListItem() {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<TransactorSelectListItem>>> transactorsJsListFunc = async () => {
                var itemsList = await _context.Transactors
                        .Include(p => p.TransactorType)
                        .Where(p => p.TransactorType.Code != "SYS.DTRANSACTOR")
                        .OrderBy(p=>p.Name)
                        .Select(p => new TransactorSelectListItem() {
                            Id = p.Id,
                            TransactorName = p.Name,
                            TransactorTypeId = p.TransactorType.Id,
                            TransactorTypeCode = p.TransactorType.Code,
                            Value = p.Id.ToString(),
                            Text = $"{p.Name} {{{p.TransactorType.Code}}}"
                        })
                        .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
            Func<Task<List<UISelectTypeItem>>> companiesListJsFunc = async () =>
            {
                var itemsList = await _context.Companies
                   .OrderBy(p=>p.Code)
                   .Select(p => new UISelectTypeItem() {
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
                    .OrderBy(p=>p.Name)
                    .AsNoTracking()
                    .ToListAsync();
                return itemsList;
            };
           
           
            //ViewData["CompanyId"] = await companiesListFunc();
            
            ViewData["TransactorId"] = await transactorListFunc();
            
            ViewData["transactorsListJs"] = await transactorsJsListFunc();
            ViewData["CurrencyListJs"] = await currenciesListJsFunc();
            ViewData["CurrencyList"]= await FiltersHelper.GetCurrenciesFilterListAsync(_context);
            ViewData["DatePeriodListJs"] = DateFilter.GetDateFiltersSelectList();
            ViewData["CompaniesListJs"] = await companiesListJsFunc();
            ViewData["AllCompaniesId"]=await FiltersHelper.GetAllCompaniesIdAsync(_context);
           
        }
    }
}
