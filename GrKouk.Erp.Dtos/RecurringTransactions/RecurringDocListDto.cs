using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringDocListDto
    {
        public int Id { get; set; }
        [Display(Name = "Frequency")]
        public string RecurringFrequency { get; set; }
        
        public RecurringDocTypeEnum RecurringDocType { get; set; }
        [Display(Name = "Doc Type")]
        public string RecurringDocTypeName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Next Date")]
        public DateTime NextTransDate { get; set; }
        [Display(Name = "Ref.")]
        public string TransRefCode { get; set; }

        public int SectionId { get; set; }
        [Display(Name = "Section")]
        public string SectionCode { get; set; }

        public int TransactorId { get; set; }
        [Display(Name = "Transactor")]
        public string TransactorName { get; set; }

        public int DocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string DocSeriesCode { get; set; }
        [Display(Name = "Series")]
        public string DocSeriesName { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [Display(Name = "Total Amount")]
        public decimal TotalAmount
        {
            get => AmountNet + AmountFpa - AmountDiscount;

        }
        [Display(Name = "Total Net Amount")]
        public decimal TotalNetAmount
        {
            get => AmountNet - AmountDiscount;

        }
        public int CompanyId { get; set; }

        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
    }
}
