using System;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.RecurringTransactions
{
    public class RecurringDocCreateAjaxNoLinesDto
    {
        public int Id { get; set; }
        public string RecurringFrequency { get; set; }
        public RecurringDocTypeEnum RecurringDocType { get; set; }
        public DateTime NextTransDate { get; set; }

        public string TransRefCode { get; set; }

        public int TransactorId { get; set; }
        public int SectionId { get; set; }
        public int DocSeriesId { get; set; }

        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
        public decimal AmountDiscount { get; set; }

        public string Etiology { get; set; }

        public int CompanyId { get; set; }
        public int PaymentMethodId { get; set; }
    }
}