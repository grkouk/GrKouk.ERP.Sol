using System;

namespace GrKouk.Erp.Dtos.SellDocuments
{
    public class SellDocCreateAjaxNoLinesDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }

        public int TransactorId { get; set; }

        public int SellDocSeriesId { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }

        public string Etiology { get; set; }
        public int PaymentMethodId { get; set; }
        public int SalesChannelId { get; set; }
        public int CompanyId { get; set; }
    }
}