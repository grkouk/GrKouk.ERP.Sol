namespace GrKouk.Web.ERP.Helpers
{
    public class TransactorIsozygioItem
    {
        public int Id { get; set; }
        public string TransactorName { get; set; }
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}