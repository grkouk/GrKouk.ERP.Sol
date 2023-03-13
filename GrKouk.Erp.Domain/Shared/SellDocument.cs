using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class SellDocument
    {
        private ICollection<SellDocLine> _sellDocLines;
        private ICollection<SellDocTransPaymentMapping> _paymentMappings;
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }
        [Required]
        public int SectionId { get; set; }
        public virtual Section Section { get; set; }
        [Required]
        public int TransactorId { get; set; }
        public virtual Transactor Transactor { get; set; }

        public int FiscalPeriodId { get; set; }
        public virtual FiscalPeriod FiscalPeriod { get; set; }
        [Required]
        public int SellDocSeriesId { get; set; }
        public virtual SellDocSeriesDef SellDocSeries { get; set; }
        [Required]
        public int SellDocTypeId { get; set; }
        public virtual SellDocTypeDef SellDocType { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountFpa { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountNet { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountDiscount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransFpaAmount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransNetAmount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransDiscountAmount { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TransExpensesAmount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public int SalesChannelId { get; set; }
        public SalesChannel SalesChannel { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
        public virtual ICollection<SellDocLine> SellDocLines
        {
            get => _sellDocLines ??= new List<SellDocLine>();
            set => _sellDocLines = value;
        }
        public virtual ICollection<SellDocTransPaymentMapping> PaymentMappings
        {
            get => _paymentMappings ??= new List<SellDocTransPaymentMapping>();
            set => _paymentMappings = value;
        }
    }
}