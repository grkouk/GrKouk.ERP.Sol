using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.RecurringTransactions
{
    public class RecurringTransDocLine
    {
        public int Id { get; set; }
        [ForeignKey("RecurringTransDoc")]
        public int RecurringTransDocId { get; set; }
        public virtual RecurringTransDoc RecurringTransDoc { get; set; }

        public int WarehouseItemId { get; set; }
        public virtual WarehouseItem WarehouseItem { get; set; }
        public int PrimaryUnitId { get; set; }
        //public virtual MeasureUnit PrimaryUnit { get; set; }
        public int SecondaryUnitId { get; set; }
        //public virtual MeasureUnit SecondaryUnit { get; set; }
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
        public decimal UnitExpenses { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountFpa { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountNet { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
    }
}
