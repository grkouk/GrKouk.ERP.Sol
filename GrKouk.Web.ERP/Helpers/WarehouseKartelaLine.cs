using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Web.ERP.Helpers
{
    public class WarehouseKartelaLine
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]

        public DateTime TransDate { get; set; }
        public string CompanyCode { get; set; }
        public string MaterialName { get; set; }
        public string DocSeriesCode { get; set; }
        public string RefCode { get; set; }
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }
        public decimal ImportVolume { get; set; }
        public decimal ExportVolume { get; set; }
        public decimal ImportValue { get; set; }
        public decimal ExportValue { get; set; }
        public decimal RunningTotalVolume { get; set; }
        public decimal RunningTotalValue { get; set; }
    }
}