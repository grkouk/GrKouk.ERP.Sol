using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocCreateDto
    {
        private ICollection<BuyDocLine> _buyDocLines;

        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Display(Name = "Section")]
        public int SectionId { get; set; }
        public string  SectionCode { get; set; }
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        public  string TransactorName { get; set; }
        [Display(Name = "Fiscal Period")]
        public int FiscalPeriodId { get; set; }
        public  string FiscalPeriodCode { get; set; }
        [Display(Name = "Doc Series")]
        public int BuyDocSeriesId { get; set; }
        public  string BuyDocSeriesCode { get; set; }
        public string BuyDocSeriesName { get; set; }

        public int BuyDocTypeId { get; set; }
        public  string BuyDocTypeCode { get; set; }
        public string BuyDocTypeName { get; set; }
        [Display(Name = "VAT Amount")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }

        [MaxLength(500)]
        public string Etiology { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
        public virtual ICollection<BuyDocLine> BuyDocLines
        {
            get { return _buyDocLines ?? (_buyDocLines = new List<BuyDocLine>()); }
            set { _buyDocLines = value; }
        }
    }
}
