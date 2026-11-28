using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.PaymentMethods
{
    /// <summary>Row of the Syncfusion index grid of payment methods.</summary>
    public class PaymentMethodListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int DaysOverdue { get; set; }
        public SeriesAutoPayoffEnum AutoPayoffWay { get; set; }
        public int? PayoffSeriesId { get; set; }
        public string PayoffSeriesName { get; set; }
        public int CfAccountId { get; set; }
        public string CfAccountName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }
}
