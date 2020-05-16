using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int DaysOverdue { get; set; }
        public SeriesAutoPayoffEnum AutoPayoffWay { get; set; }
        public int? PayoffSeriesId { get; set; }
    }

    public class FinancialAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
