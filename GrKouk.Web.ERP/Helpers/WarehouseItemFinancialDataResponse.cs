using System;

namespace GrKouk.Web.ERP.Helpers
{
    public class WarehouseItemFinancialDataResponse
    {
        public decimal SumImportVolume { get; set; }
        public decimal SumImportValue { get; set; }
        public decimal SumExportVolume { get; set; }
        public decimal SumExportValue { get; set; }
        public DateTime LastDebitDate { get; set; }
        public DateTime LastCreditDate { get; set; }
    }
}