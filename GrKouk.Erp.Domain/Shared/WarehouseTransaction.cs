using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class WarehouseTransaction
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public int TransWarehouseDocSeriesId { get; set; }
        public virtual TransWarehouseDocSeriesDef TransWarehouseDocSeries { get; set; }

        public int TransWarehouseDocTypeId { get; set; }
        public virtual TransWarehouseDocTypeDef TransWarehouseDocType { get; set; }
        /// <summary>
        /// Αριθμός Παραστατικού
        /// </summary>
        public string TransRefCode { get; set; }

        public int SectionId { get; set; }
        public virtual Section Section { get; set; }

        public int WarehouseItemId { get; set; }
        public virtual WarehouseItem WarehouseItem { get; set; }

        public int FiscalPeriodId { get; set; }
        public virtual FiscalPeriod FiscalPeriod { get; set; }

        /// <summary>
        /// Debit or Credit, for warehouse Import or Export
        /// </summary>
        public WarehouseTransactionTypeEnum TransactionType { get; set; }

        public InventoryActionEnum InventoryAction { get; set; }
        public InventoryValueActionEnum InventoryValueAction { get; set; }
        //Added 2019-------------
        public InventoryActionEnum InvoicedVolumeAction { get; set; }
        public InventoryValueActionEnum InvoicedValueAction { get; set; }
        //----------------------
        /// <summary>
        /// Ανάλογα με το sectionid το πεδίο αυτό περιέχει το id του αντικειμένου που ορίζει το sectionID 
        /// </summary>
        public int CreatorId { get; set; }
        public int PrimaryUnitId { get; set; }
        //public virtual MeasureUnit PrimaryUnit { get; set; }
        public int SecondaryUnitId { get; set; }
        //public virtual MeasureUnit SecondaryUnit { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal UnitFactor { get; set; }
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
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransQ1 { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransQ2 { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransFpaAmount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransNetAmount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransDiscountAmount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }

    }
}