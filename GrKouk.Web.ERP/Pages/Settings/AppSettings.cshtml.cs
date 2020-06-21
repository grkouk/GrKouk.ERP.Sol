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
        private readonly List<AppSetting> _settingsToUse;

        public AppSettingsModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
            _settingsToUse = new List<AppSetting>
            {
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.AllCompaniesCodeKey, Value = "ALLCOMP"},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialBuys, Value = ""},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfExpenseBuys, Value = ""},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfServiceBuys, Value = ""},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfMaterialSales, Value = ""},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfServiceSales, Value = ""},
                new AppSetting {Code = GrKouk.Erp.Definitions.Constants.MainInfoPageSumOfIncomeSales, Value = ""}
            };
        }

        [BindProperty] public List<AppSetting> ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            LoadCombos();
            ItemVm = new List<AppSetting>();

            for (int i = 0; i < _settingsToUse.Count; i++)
            {
                var setting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == _settingsToUse[i].Code);
                if (setting != null)
                {
                    var acs = new AppSetting
                    {
                        Code = setting.Code,
                        Value = setting.Value,
                    };
                    ItemVm.Add(acs);
                }
                else
                {
                    ItemVm.Add(new AppSetting
                    {
                        Code = _settingsToUse[i].Code,
                        Value = _settingsToUse[i].Value
                    });
                }
            }

            return Page();
        }

        private void LoadCombos()
        {
            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(p => p.Name).AsNoTracking(), "Code", "Name");
            var sourceTypeList = Enum.GetValues(typeof(MainInfoSourceTypeEnum))
                .Cast<MainInfoSourceTypeEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = ((int) c).ToString(),
                    Text = c.GetDescription()
                }).ToList();
            ViewData["SourceType"] = sourceTypeList;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            for (int i = 0; i < _settingsToUse.Count; i++)
            {
                var setting = await _context.AppSettings.FirstOrDefaultAsync(p => p.Code == _settingsToUse[i].Code);
                if (setting != null)
                {
                    setting.Value = ItemVm[i].Value;
                    _context.Attach(setting).State = EntityState.Modified;
                }
                else
                {
                    _context.AppSettings.Add(_settingsToUse[i]);
                }
            }

            try
            {
                var iRec = await _context.SaveChangesAsync();
                var ms = $"Updated {iRec} settings";
                _toastNotification.AddSuccessToastMessage(ms);
                return Page();
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