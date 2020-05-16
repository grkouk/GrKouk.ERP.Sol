using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocCreateAjaxDto
    {
        private IList<SellDocLineAjaxDto> _sellDocLines;

        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Trans Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Required]
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        [Required]
        [Display(Name = "Doc Series")]
        public int SellDocSeriesId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
        [Required]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }
        [Display(Name = "Sales Channel")]
        public int SalesChannelId { get; set; }
        //public Single Q1 { get; set; }
        //public Single Q2 { get; set; }

        public virtual IList<SellDocLineAjaxDto> SellDocLines
        {
            get { return _sellDocLines ?? (_sellDocLines = new List<SellDocLineAjaxDto>()); }
            set { _sellDocLines = value; }
        }
    }
}
