using System;

namespace GrKouk.Erp.Dtos.CashRegister
{
    public class SupplierLedgerRowDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }
        public string DocSeriesName { get; set; }
        public string TransRefCode { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
