using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class BuyDocument
    {
        private ICollection<BuyDocLine> _buyDocLines;
        private ICollection<BuyDocTransPaymentMapping> _paymentMappings;
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
        public int BuyDocSeriesId { get; set; }
        public virtual BuyDocSeriesDef BuyDocSeries { get; set; }
        [Required]
        public int BuyDocTypeId { get; set; }
        public virtual BuyDocTypeDef BuyDocType { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountFpa { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountNet { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountDiscount { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountExpenses { get; set; }

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

        [Timestamp]
        public byte[] Timestamp { get; set; }
        public virtual ICollection<BuyDocLine> BuyDocLines
        {
            get => _buyDocLines ?? (_buyDocLines = new List<BuyDocLine>());
            set => _buyDocLines = value;
        }
        public virtual ICollection<BuyDocTransPaymentMapping> PaymentMappings
        {
            get => _paymentMappings ?? (_paymentMappings = new List<BuyDocTransPaymentMapping>());
            set => _paymentMappings = value;
        }
    }
}
