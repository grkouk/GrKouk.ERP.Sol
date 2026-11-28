using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.PaymentMethods
{
    /// <summary>
    /// Which payment methods are used by documents. Deleting a used method must be refused:
    /// buy and sell documents reference it with cascade delete, so they would be deleted with it.
    /// </summary>
    public static class PaymentMethodUsage
    {
        /// <summary>Codes of the given methods that buy, sell or recurring documents use.</summary>
        public static async Task<List<string>> GetUsedCodesAsync(ApiDbContext context, IReadOnlyCollection<int> ids)
        {
            var usedIds = await context.BuyDocuments.Where(d => ids.Contains(d.PaymentMethodId)).Select(d => d.PaymentMethodId)
                .Union(context.SellDocuments.Where(d => ids.Contains(d.PaymentMethodId)).Select(d => d.PaymentMethodId))
                .Union(context.RecurringTransDocs.Where(d => ids.Contains(d.PaymentMethodId)).Select(d => d.PaymentMethodId))
                .ToListAsync();
            if (usedIds.Count == 0)
            {
                return new List<string>();
            }

            return await context.PaymentMethods
                .Where(p => usedIds.Contains(p.Id))
                .OrderBy(p => p.Code)
                .Select(p => p.Code)
                .ToListAsync();
        }
    }
}
