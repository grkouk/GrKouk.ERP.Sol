using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    public class BuyDocLine
    {
        public int Id { get; set; }
        [ForeignKey("BuyDocument")]
        public int BuyDocumentId { get; set; }
        public virtual BuyDocument BuyDocument { get; set; }

        public int WarehouseItemId { get; set; }
        public virtual WarehouseItem WarehouseItem { get; set; }
        public int PrimaryUnitId { get; set; }
        public int SecondaryUnitId { get; set; }
        //public virtual MeasureUnit SecondaryUnit { get; set; }
        //Αλλαγή της εισαγωγής σε νουμερα στην μοναδα κίνησης.
        //Δηλ 10 συσκευασίες με τιμή συσκευασίας και απο αυτά υπολογισμός σε μοναδα βάσης
        public int TransactionUnitId { get; set; }
        public double TransactionQuantity { get; set; }
        public Single TransactionUnitFactor { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransUnitPrice { get; set; }
        public Single Factor { get; set; }
        /// <summary>
        /// Ποσότητα σε μονάδα μέτρησης 1
        /// </summary>
        public double Quontity1 { get; set; }
        /// <summary>
        /// Ποσότητα σε μονάδα μέτρησης 2
        /// </summary>
        public double Quontity2 { get; set; }
        public decimal FpaRate { get; set; }
        public decimal DiscountRate { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountFpa { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountNet { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountDiscount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountExpenses { get; set; }
    
        [MaxLength(500)]
        public string Etiology { get; set; }
    }
}
