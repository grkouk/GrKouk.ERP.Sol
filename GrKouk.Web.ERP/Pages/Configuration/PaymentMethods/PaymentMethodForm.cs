using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    /// <summary>The dropdown lists and checks shared by the Create and Edit pages of payment methods.</summary>
    public class PaymentMethodForm
    {
        public List<SelectListItem> AutoPayoffWays { get; private set; }
        public List<SelectListItem> CfAccounts { get; private set; }
        public List<SelectListItem> PayoffSeries { get; private set; }
        public List<SelectListItem> Companies { get; private set; }

        public static async Task<PaymentMethodForm> LoadAsync(ApiDbContext context)
        {
            var payoffSeries = await context.TransTransactorDocSeriesDefs.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
            // Empty value binds to null: no payoff series.
            payoffSeries.Insert(0, new SelectListItem { Value = "", Text = "{No Selection}" });

            return new PaymentMethodForm
            {
                AutoPayoffWays = Enum.GetValues<SeriesAutoPayoffEnum>()
                    .Select(c => new SelectListItem { Value = c.ToString(), Text = c.GetDescription() })
                    .ToList(),
                CfAccounts = await SelectListHelpers.GetCfAccountsNoSelectionListAsync(context),
                PayoffSeries = payoffSeries,
                Companies = await context.Companies.AsNoTracking()
                    .OrderBy(c => c.Code)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                    .ToListAsync()
            };
        }

        /// <summary>
        /// The {Ολες} (all companies) company, the default for a new method: the cash register lists
        /// the methods of its company or of all companies. 0 when the ALLCOMPCODE setting is missing.
        /// </summary>
        public static async Task<int> GetAllCompaniesIdAsync(ApiDbContext context)
        {
            var allCompaniesCode = await context.AppSettings
                .Where(p => p.Code == Constants.AllCompaniesCodeKey)
                .Select(p => p.Value)
                .SingleOrDefaultAsync();
            return await context.Companies
                .Where(c => c.Code == allCompaniesCode)
                .Select(c => c.Id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Checks what the database would reject or the rest of the app needs: a unique code
        /// and an existing company. Errors go on the "ItemVm.*" fields of the form.
        /// </summary>
        public static async Task ValidateAsync(ApiDbContext context, ModelStateDictionary modelState,
            int id, string code, int companyId)
        {
            if (!string.IsNullOrWhiteSpace(code)
                && await context.PaymentMethods.AnyAsync(p => p.Code == code && p.Id != id))
            {
                modelState.AddModelError("ItemVm.Code", "Υπάρχει ήδη τρόπος πληρωμής με αυτόν τον κωδικό.");
            }
            if (!await context.Companies.AnyAsync(c => c.Id == companyId))
            {
                modelState.AddModelError("ItemVm.CompanyId", "Επιλέξτε εταιρεία.");
            }
        }
    }
}
