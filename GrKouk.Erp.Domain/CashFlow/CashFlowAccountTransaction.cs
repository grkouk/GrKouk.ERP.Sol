using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.CashFlow
{
    public class CashFlowAccountTransaction
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public int DocumentSeriesId { get; set; }
        public virtual CashFlowDocSeriesDef DocumentSeries { get; set; }

        public int DocumentTypeId { get; set; }
        public virtual CashFlowDocTypeDef DocumentType { get; set; }

        public string RefCode { get; set; }
        public int CashFlowAccountId { get; set; }
        public virtual CashFlowAccount CashFlowAccount { get; set; }

        public int SectionId { get; set; }
        public virtual Section Section { get; set; }
        public int CreatorId { get; set; }
        public int CreatorSectionId { get; set; }
        public int FiscalPeriodId { get; set; }
        public virtual FiscalPeriod FiscalPeriod { get; set; }
      
        public FinActionsEnum FinancialAction { get; set; }

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

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}