using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocModifyAjaxDto
    {
        private IList<SellDocLineAjaxDto> _sellDocLines;

        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }
        [Required]
        public int TransactorId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public int SellDocSeriesId { get; set; }
        public int SellDocTypeId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }
        public int PaymentMethodId { get; set; }
        public int SalesChannelId { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public int FiscalPeriodId { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }

        public virtual IList<SellDocLineAjaxDto> SellDocLines
        {
            get { return _sellDocLines ?? (_sellDocLines = new List<SellDocLineAjaxDto>()); }
            set { _sellDocLines = value; }
        }
    }
}
