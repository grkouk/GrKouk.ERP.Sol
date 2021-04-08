using System;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocLineAjaxDto
    {
        public int WarehouseItemId { get; set; }
        public int TransactionUnitId { get; set; }
        public double TransactionQuantity { get; set; }
        public Single TransactionUnitFactor { get; set; }
        public decimal TransUnitPrice { get; set; }
        public double Q1 { get; set; }
        public double Q2 { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountDiscount { get; set; }
        public decimal AmountExpenses { get; set; }
        public Single DiscountRate { get; set; }
        public int MainUnitId { get; set; }
        public int SecUnitId { get; set; }
        public Single Factor { get; set; }
        public Single FpaRate { get; set; }
    }
}
