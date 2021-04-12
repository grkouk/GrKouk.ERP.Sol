using System;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocLineModifyDto
    {
        public int Id { get; set; }

        public int BuyDocumentId { get; set; }

        public int WarehouseItemId { get; set; }
        public string WarehouseItemName { get; set; }

        public int PrimaryUnitId { get; set; }
        public string PrimaryUnitName { get; set; }
        public string PrimaryUnitCode { get; set; }
        public int SecondaryUnitId { get; set; }
        public string SecondaryUnitName { get; set; }
        public string SecondaryUnitCode { get; set; }
        public int TransactionUnitId { get; set; }
        public string TransactionUnitName { get; set; }
        public string TransactionUnitCode { get; set; }
        public double TransactionQuantity { get; set; }
        public Single TransactionUnitFactor { get; set; }
        public Single Factor { get; set; }
        public double Quontity1 { get; set; }
        public double Quontity2 { get; set; }
        public decimal FpaRate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        public decimal TransUnitPrice { get; set; }
        public decimal AmountExpenses { get; set; }
        public string Etiology { get; set; }
    }
}