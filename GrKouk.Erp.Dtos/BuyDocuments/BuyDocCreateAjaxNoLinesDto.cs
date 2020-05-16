using System;

namespace GrKouk.Erp.Dtos.BuyDocuments
{
    public class BuyDocCreateAjaxNoLinesDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }

        public string TransRefCode { get; set; }
       
        public int TransactorId { get; set; }
       
        public int BuyDocSeriesId { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }
       
        public string Etiology { get; set; }
       
        public int CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
    }
}