using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Web.ERP.Helpers
{
    public class KartelaLine
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]

        public DateTime TransDate { get; set; }

        public string TransactorName { get; set; }
        public string CompanyCode { get; set; }
        public string DocSeriesCode { get; set; }
        public string RefCode { get; set; }
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningTotal { get; set; }
    }
}