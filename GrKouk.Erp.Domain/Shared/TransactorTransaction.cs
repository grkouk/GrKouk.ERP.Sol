using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class TransactorTransaction
    {
        private ICollection<BuyDocTransPaymentMapping> _buyDocPaymentMappings;
        private ICollection<SellDocTransPaymentMapping> _salesDocPaymentMappings;
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public int TransTransactorDocSeriesId { get; set; }
        public virtual TransTransactorDocSeriesDef TransTransactorDocSeries { get; set; }

        public int TransTransactorDocTypeId { get; set; }
        public virtual TransTransactorDocTypeDef TransTransactorDocType { get; set; }

        public string TransRefCode { get; set; }
        public int TransactorId { get; set; }
        public virtual Transactor Transactor { get; set; }

        public int SectionId { get; set; }
        public virtual Section Section { get; set; }
        public int CreatorId { get; set; }
        public int CreatorSectionId { get; set; }
        public int FiscalPeriodId { get; set; }
        public virtual FiscalPeriod FiscalPeriod { get; set; }

        public FinActionsEnum FinancialAction { get; set; }

        public decimal FpaRate { get; set; }

        public decimal DiscountRate { get; set; }
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

        [MaxLength(500)]
        public string Etiology { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public int CfAccountId { get; set; }

        public virtual ICollection<BuyDocTransPaymentMapping> BuyDocPaymentMappings
        {
            get => _buyDocPaymentMappings ??= new List<BuyDocTransPaymentMapping>();
            set => _buyDocPaymentMappings = value;
        }
        public virtual ICollection<SellDocTransPaymentMapping> SalesDocPaymentMappings
        {
            get => _salesDocPaymentMappings ??= new List<SellDocTransPaymentMapping>();
            set => _salesDocPaymentMappings = value;
        }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}