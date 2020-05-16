using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocModifyAjaxNoLinesDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }

        public int TransactorId { get; set; }

        public int BuyDocSeriesId { get; set; }
        public int BuyDocTypeId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }

        public string Etiology { get; set; }
        public int FiscalPeriodId { get; set; }
        public int PaymentMethodId { get; set; }
        public int CompanyId { get; set; }
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}