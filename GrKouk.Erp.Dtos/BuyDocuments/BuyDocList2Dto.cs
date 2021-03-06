﻿using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocList2Dto
    {
        //private ICollection<BuyDocLine> _buyDocLines;

        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Ref.")]
        public string TransRefCode { get; set; }

        public int SectionId { get; set; }
        [Display(Name = "Section")]
        public string SectionCode { get; set; }

        public int TransactorId { get; set; }
        [Display(Name = "Transactor")]
        public string TransactorName { get; set; }

        public int BuyDocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string BuyDocSeriesCode { get; set; }
        [Display(Name = "Series")]
        public string BuyDocSeriesName { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => AmountNet + AmountFpa - AmountDiscount;

        [Display(Name = "Total Net Amount")]
        public decimal TotalNetAmount => AmountNet - AmountDiscount;

        public int CompanyId { get; set; }

        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public decimal PayedOfAmount { get; set; }

        public decimal UncoveredAmount => PayedOfAmount - TotalAmount;
    }
}