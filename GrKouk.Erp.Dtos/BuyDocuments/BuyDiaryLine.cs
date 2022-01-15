using GrKouk.Erp.Definitions;
using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    /// <summary>
    /// Class used in Buy Diaries
    /// </summary>
    public class BuyDiaryLine
    {
        public int Id { get; set; }
        public int DocId { get; set; }
        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }
        public string RefCode { get; set; }
        public int DocSeriesId { get; set; }
        public string DocSeriesName { get; set; }

        public string DocSeriesCode { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public string DocTypeCode { get; set; }
        public int TransactorId { get; set; }
        public string TransactorName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int SectionId { get; set; }
        public string SectionCode { get; set; }
        public int CreatorSectionId { get; set; }
        public string CreatorSectionCode { get; set; }
        public int ItemNature { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
        public decimal AmountExpenses { get; set; }
        public decimal TransFpaAmount { get; set; }
        public decimal TransNetAmount { get; set; }
        public decimal TransDiscountAmount { get; set; }
        public decimal TransExpencesAmount { get; set; }

        public int CompanyId { get; set; }

        //[Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public decimal TotalNetAmount => TransNetAmount - TransDiscountAmount;

        public decimal TotalAmount => TransNetAmount + TransFpaAmount - TransDiscountAmount;
    }
}