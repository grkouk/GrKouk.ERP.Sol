using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Web.ERP.Helpers
{
    public class CfaKartelaLine
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]

        public DateTime TransDate { get; set; }

        public string CahsFlowAccountName { get; set; }
        public string CompanyCode { get; set; }
        public string DocSeriesCode { get; set; }
        public string DocSeriesName { get; set; }
        public string RefCode { get; set; }
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }
        public int CreatorSectionId { get; set; }
        public string CreatorSectionCode { get; set; }
        public decimal Deposit { get; set; }
        public decimal Withdraw { get; set; }
        public decimal RunningTotal { get; set; }
    }
}