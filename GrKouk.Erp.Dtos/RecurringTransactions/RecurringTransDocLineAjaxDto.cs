namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringTransDocLineAjaxDto
    {
        public int WarehouseItemId { get; set; }
        public double Q1 { get; set; }
        public double Q2 { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public float DiscountRate { get; set; }
        public int MainUnitId { get; set; }
        public int SecUnitId { get; set; }
        public float Factor { get; set; }
        public float FpaRate { get; set; }

    }
}
