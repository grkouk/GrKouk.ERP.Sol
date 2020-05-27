using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Settings
{
    public class AppSettingsModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;

        public AppSettingsModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        [BindProperty]
        public List<AppSetting> ItemVm { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            LoadCombos();
            ItemVm= new List<AppSetting>();
           
            var allCompanyCodeSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey);
            if (allCompanyCodeSetting != null)
            {
                var acs = new AppSetting
                {
                    Code = allCompanyCodeSetting.Code,
                    Value = allCompanyCodeSetting.Value,
                };
                ItemVm.Add(acs);
            }
            else
            {
                ItemVm.Add(new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey,
                    Value = "ALLCOMP"
                });
            }
            var sumOfMaterialBuysSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialBuys);
            if (sumOfMaterialBuysSetting != null)
            {
                var acs = new AppSetting
                {
                    Code = sumOfMaterialBuysSetting.Code,
                    Value = sumOfMaterialBuysSetting.Value
                };
                ItemVm.Add(acs);
            }
            else
            {
                ItemVm.Add(new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialBuys,
                    Value = ""
                });
            }

            var sumOfExpensesBuysSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfExpenseBuys);
            if (sumOfExpensesBuysSetting != null)
            {
                var acs = new AppSetting
                {
                    Code = sumOfExpensesBuysSetting.Code,
                    Value = sumOfExpensesBuysSetting.Value
                };
                ItemVm.Add(acs);
            }
            else
            {
                ItemVm.Add(new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfExpenseBuys,
                    Value = ""
                });
            }
            var sumOfMaterialSalesSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialSales);
            if (sumOfMaterialSalesSetting != null)
            {
                var acs = new AppSetting
                {
                    Code = sumOfMaterialSalesSetting.Code,
                    Value = sumOfMaterialSalesSetting.Value
                };
                ItemVm.Add(acs);
            }
            else
            {
                ItemVm.Add(new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialSales,
                    Value = ""
                });
            }
            return Page();
        }

        private void LoadCombos()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Name).AsNoTracking(), "Code", "Name");
            var sourceTypeList = Enum.GetValues(typeof(MainInfoSourceTypeEnum))
                .Cast<MainInfoSourceTypeEnum>()
                .Select(c => new SelectListItem()
                {
                    
                    Value = ((int)c).ToString(),
                    Text = c.GetDescription()
                }).ToList();
            ViewData["SourceType"] = sourceTypeList;
        }

        public async Task<IActionResult> OnPostAsync()
        {
           
            var allCompanyCodeSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey);
            if (allCompanyCodeSetting != null)
            {
                allCompanyCodeSetting.Value=ItemVm[0].Value;
                _context.Attach(allCompanyCodeSetting).State = EntityState.Modified;
            }
            else
            {
                var newAllCompSetting = new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey,
                    Value = ItemVm[0].Value
                };
                _context.AppSettings.Add(newAllCompSetting);
            }
            var buyMaterialsSumSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialBuys);
            if (buyMaterialsSumSetting != null)
            {
                buyMaterialsSumSetting.Value=ItemVm[1].Value;
                _context.Attach(buyMaterialsSumSetting).State = EntityState.Modified;
            }
            else
            {
                var newBuyMaterialsSumSetting = new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialBuys,
                    Value = ItemVm[1].Value
                };
                _context.AppSettings.Add(newBuyMaterialsSumSetting);
            }
            
            var buyExpensesSumSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfExpenseBuys);
            if (buyExpensesSumSetting != null)
            {
                buyExpensesSumSetting.Value=ItemVm[2].Value;
                _context.Attach(buyExpensesSumSetting).State = EntityState.Modified;
            }
            else
            {
                var newBuyExpensesSumSetting = new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfExpenseBuys,
                    Value = ItemVm[2].Value
                };
                _context.AppSettings.Add(newBuyExpensesSumSetting);
            }
            var salesMaterialsSumSetting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialSales);
            if (salesMaterialsSumSetting != null)
            {
                salesMaterialsSumSetting.Value=ItemVm[3].Value;
                _context.Attach(salesMaterialsSumSetting).State = EntityState.Modified;
            }
            else
            {
                var newSetting = new AppSetting
                {
                    Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialSales,
                    Value = ItemVm[3].Value
                };
                _context.AppSettings.Add(newSetting);
            }
            
            try
            {
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Settings saved");
                return RedirectToPage("./AppSettings");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _toastNotification.AddErrorToastMessage(e.Message);
                return Page();
            }

           
        }
    }
}