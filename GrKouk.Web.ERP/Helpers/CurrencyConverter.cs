using System;
using System.Collections.Generic;
using System.Linq;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Web.ERP.Helpers
{
    public static class CurrencyConverter
    {
        /// <summary>
        /// Converts an amount from company currency to display currency using provided exchange rates.
        /// Assumes currency with Id = baseCurrencyId is the base currency (default 1).
        /// If no conversion is required or rates are missing, returns the original amount.
        /// Result is rounded to 4 decimals to match decimal(18,4) schema.
        /// </summary>
        public static decimal ConvertAmount(int companyCurrencyId, int displayCurrencyId, IList<ExchangeRate> rates, decimal amount)
        {
            const int baseCurrencyId = 1;
            if (displayCurrencyId == companyCurrencyId)
            {
                return amount;
            }

            decimal retAmount = amount;

            // Convert company currency -> base currency
            if (companyCurrencyId != baseCurrencyId)
            {
                var r = rates.Where(p => p.CurrencyId == companyCurrencyId)
                    .OrderByDescending(p => p.ClosingDate)
                    .FirstOrDefault();
                if (r == null || r.Rate == 0)
                {
                    return Math.Round(retAmount, 4); // no rate; return as-is rounded
                }
                retAmount = amount / r.Rate;
            }

            // Convert base currency -> display currency
            if (displayCurrencyId != baseCurrencyId)
            {
                var r = rates.Where(p => p.CurrencyId == displayCurrencyId)
                    .OrderByDescending(p => p.ClosingDate)
                    .FirstOrDefault();
                if (r == null)
                {
                    return Math.Round(retAmount, 4); // no rate; return base amount rounded
                }
                retAmount *= r.Rate;
            }

            return Math.Round(retAmount, 4);
        }
    }
}