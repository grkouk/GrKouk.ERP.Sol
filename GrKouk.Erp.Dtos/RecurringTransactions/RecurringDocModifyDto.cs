using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringDocModifyDto
    {
        private ICollection<RecurringDocLineModifyDto> _buyDocLines;

        public int Id { get; set; }
        [MaxLength(2)]
        public string RecurringFrequency { get; set; }
        public RecurringDocTypeEnum RecurringDocType { get; set; }
        [Display(Name = "Next Trans Date")]
        [DataType(DataType.Date)]
        public DateTime NextTransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        //[Display(Name = "Section")]
        //public int SectionId { get; set; }
        //public string SectionCode { get; set; }
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        public string TransactorName { get; set; }
        public int SectionId { get; set; }
        [Display(Name = "Doc Series")]
        public int DocSeriesId { get; set; }

        public string DocSeriesCode { get; set; }
        [Display(Name = "Series Name")]
        public string DocSeriesName { get; set; }

        //public int BuyDocTypeId { get; set; }
        //public string BuyDocTypeCode { get; set; }
        //public string BuyDocTypeName { get; set; }
        [Display(Name = "Vat Amount")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Sum Amount")]
        public decimal AmountSum => AmountNet + AmountFpa - AmountDiscount;

        [MaxLength(500)]
        public string Etiology { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }

        [Timestamp] public byte[] Timestamp { get; set; }

        public virtual ICollection<RecurringDocLineModifyDto> BuyDocLines
        {
            get { return _buyDocLines ??= new List<RecurringDocLineModifyDto>(); }
            set { _buyDocLines = value; }
        }
    }
}
