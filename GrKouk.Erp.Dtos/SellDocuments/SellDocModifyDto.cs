using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.SellDocuments
{
   public class SellDocModifyDto
    {
        private ICollection<SellDocLineModifyDto> _sellDocLines;

        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }

        public int SectionId { get; set; }
        public string SectionCode { get; set; }
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        public string TransactorName { get; set; }

        public int FiscalPeriodId { get; set; }
        public string FiscalPeriodCode { get; set; }
        [Display(Name = "Doc Series")]
        public int SellDocSeriesId { get; set; }
        public string SellDocSeriesCode { get; set; }
        public string SellDocSeriesName { get; set; }

        public int SellDocTypeId { get; set; }
        public string SellDocTypeCode { get; set; }
        public string SellDocTypeName { get; set; }
        [Display(Name = "Vat Amount")] 
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")] 
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")] 
        public decimal AmountDiscount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Sum")]
        public decimal AmountSum => (AmountNet + AmountFpa - AmountDiscount);

        [MaxLength(500)] public string Etiology { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }
        [Display(Name = "Sales Channel")]
        public int SalesChannelId { get; set; }
        [Timestamp] public byte[] Timestamp { get; set; }

        public virtual ICollection<SellDocLineModifyDto> SellDocLines
        {
            get { return _sellDocLines ?? (_sellDocLines = new List<SellDocLineModifyDto>()); }
            set { _sellDocLines = value; }
        }
    }
}
