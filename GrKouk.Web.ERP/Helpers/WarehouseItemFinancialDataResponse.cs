using System;

namespace GrKouk.Web.ERP.Helpers
{
    public class WarehouseItemFinancialDataResponse
    {
        public decimal SumImportVolume { get; set; }
        public decimal SumImportValue { get; set; }
        public decimal SumExportVolume { get; set; }
        public decimal SumExportValue { get; set; }
        public decimal SumInvoicedImportVolume { get; set; }
        public decimal SumInvoicedImportValue { get; set; }
        public decimal SumInvoicedExportVolume { get; set; }
        public decimal SumInvoicedExportValue { get; set; }
        public decimal MediumInvoicedExportPrice { 
            get
                {
                if (SumInvoicedExportVolume == 0)
                {
                    return 0;
                }
                else
                {
                    return SumInvoicedExportValue / SumInvoicedExportVolume;
                }
            } 
            }
        public decimal MediumInvoicedImportPrice
        {
            get
            {
                if (SumInvoicedImportVolume==0)
                {
                    return 0;
                }else
                {
                    return SumInvoicedImportValue/SumInvoicedImportVolume;
                }
            }
        }
        public decimal VolumeBalance => SumImportVolume - SumExportVolume;
        public decimal ValueBalance => SumImportValue-SumExportValue;
        public DateTime LastDebitDate { get; set; }
        public DateTime LastCreditDate { get; set; }
    }
}