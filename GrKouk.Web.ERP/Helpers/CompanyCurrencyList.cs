﻿namespace GrKouk.Web.ERP.Helpers
{
    public class CompanyCurrencyList
    {
        public int CompanyId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        public string DisplayLocale { get; set; }
        public string CurrencyName { get; set; }
    }
}
