﻿using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocListDto
    {
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

        public int SellDocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string SellDocSeriesCode { get; set; }
        [Display(Name = "Series")]
        public string SellDocSeriesName { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        public decimal TransFpaAmount { get; set; }
        public decimal TransNetAmount { get; set; }
        public decimal TransDiscountAmount { get; set; }
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => AmountNet + AmountFpa - AmountDiscount;

        [Display(Name = "Total Net Amount")]
        public decimal TotalNetAmount => AmountNet - AmountDiscount;
        [Display(Name = "Trans Total Amount")]
        public decimal TransTotalAmount
        {
            get => TransNetAmount + TransFpaAmount - TransDiscountAmount;

        }
        [Display(Name = "Trans Total Net Amount")]
        public decimal TransTotalNetAmount
        {
            get => TransNetAmount - TransDiscountAmount;

        }
        public int CompanyId { get; set; }

        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public int SalesChannelId { get; set; }
    }
}
