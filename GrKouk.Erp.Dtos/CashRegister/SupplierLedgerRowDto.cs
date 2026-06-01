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
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }

        /// <summary>True when this ledger row is an editable supplier payment (a transaction in
        /// the payment section). For such rows <see cref="Id"/> is the supplier-payment id usable
        /// with the supplier-payments edit/delete endpoints.</summary>
        public bool IsPayment { get; set; }
    }
}
