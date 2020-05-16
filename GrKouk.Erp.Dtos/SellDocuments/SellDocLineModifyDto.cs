using System;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocLineModifyDto
    {
        public int Id { get; set; }

        public int SellDocumentId { get; set; }

        public int WarehouseItemId { get; set; }
        public string WarehouseItemName { get; set; }

        public int PrimaryUnitId { get; set; }
        public int SecondaryUnitId { get; set; }
        public Single Factor { get; set; }
        public double Quontity1 { get; set; }
        public double Quontity2 { get; set; }
        public decimal FpaRate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        public string Etiology { get; set; }
    }
}
