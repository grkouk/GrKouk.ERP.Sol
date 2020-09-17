namespace GrKouk.Web.ERP.Helpers
{
    public class TransactorSelectListItem    
    {
        public int Id { get; set; }
        public string TransactorName { get; set; }
        public int TransactorTypeId { get; set; }
        public string TransactorTypeCode { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class TransactorDocTypeAllowedTransactorTypes
    {
        public int DocSeriesId { get; set; }
        public int DocTypeId { get; set; }
        public string[] AllowedTypesArray { get; set; }
        public string AllowedTypes { get; set; }
    }
}