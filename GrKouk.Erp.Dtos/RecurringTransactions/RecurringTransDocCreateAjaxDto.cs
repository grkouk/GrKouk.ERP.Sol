using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringTransDocCreateAjaxDto
    {
        private IList<RecurringTransDocLineAjaxDto> _docLines;

        public int Id { get; set; }
        public string RecurringFrequency { get; set; }
        [Display(Name = "Doc Type")]
        public RecurringDocTypeEnum RecurringDocType { get; set; }
        
        [Required]
        [Display(Name = "Next Trans Date")]
        [DataType(DataType.Date)]
        public DateTime NextTransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Required]
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        public int SectionId { get; set; }
        [Required]
        [Display(Name = "Doc Series")]
        public int DocSeriesId { get; set; }
        [Display(Name = "VAT Amount")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }
        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

      

        public virtual IList<RecurringTransDocLineAjaxDto> DocLines
        {
            get { return _docLines ?? (_docLines = new List<RecurringTransDocLineAjaxDto>()); }
            set { _docLines = value; }
        }
    }
}