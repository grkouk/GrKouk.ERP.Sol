using System;

namespace GrKouk.Erp.Dtos.CashRegister
{
    /// <summary>
    /// A single open (unpaid-after-FIFO) supplier invoice for the open-item payables
    /// ledger. OpenAmount is the remaining payable after FIFO allocation of pooled
    /// payments across the selected branches. DueDate = invoice date + payment-method
    /// credit term (DaysOverdue).
    /// </summary>
    public class SupplierOpenItemDto
    {
        public int BuyDocumentId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DocSeriesName { get; set; }
        public string DocRef { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal OpenAmount { get; set; }
        public int DaysPastDue { get; set; }
    }
}
