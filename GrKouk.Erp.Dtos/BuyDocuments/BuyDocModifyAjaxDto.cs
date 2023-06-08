using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocModifyAjaxDto
    {
        private IList<BuyDocLineAjaxDto> _buyDocLines;

        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }
        [Required]
        public int TransactorId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public int BuyDocSeriesId { get; set; }
        public int BuyDocTypeId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
        public int FiscalPeriodId { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }

        public virtual IList<BuyDocLineAjaxDto> BuyDocLines
        {
            get { return _buyDocLines ??= new List<BuyDocLineAjaxDto>(); }
            set { _buyDocLines = value; }
        }
    }
}