using System;

namespace GrKouk.Erp.Dtos.CashRegister
{
    public class SupplierPaymentListItemDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }
        public int TransactorId { get; set; }
        public string TransactorName { get; set; }
        public string TransactorCode { get; set; }
        public string DocSeriesName { get; set; }
        public string DocTypeName { get; set; }
        public string TransRefCode { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountDiscount { get; set; }
        public decimal AmountSum { get; set; }
        public string Etiology { get; set; }
        public byte[] Timestamp { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }
}
