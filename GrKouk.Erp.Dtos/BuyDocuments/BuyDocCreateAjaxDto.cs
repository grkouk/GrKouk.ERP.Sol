using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocCreateAjaxDto
    {
        private IList<BuyDocLineAjaxDto> _buyDocLines;

        public int Id { get; set; }
        [Required]
        [Display(Name = "Trans Date")]
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }
        [Display(Name = "Ref Code")]
        public string TransRefCode { get; set; }
        [Required]
        [Display(Name = "Transactor")]
        public int TransactorId { get; set; }
        [Required]
        [Display(Name = "Doc Series")]
        public int BuyDocSeriesId { get; set; }
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

        //public Single Q1 { get; set; }
        //public Single Q2 { get; set; }

        public virtual IList<BuyDocLineAjaxDto> BuyDocLines
        {
            get { return _buyDocLines ??= new List<BuyDocLineAjaxDto>(); }
            set { _buyDocLines = value; }
        }
    }
}