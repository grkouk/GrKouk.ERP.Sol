namespace GrKouk.Web.ERP.Helpers
{
    public class WarehouseIsozygioItem
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public decimal ImportVolume { get; set; }
        public decimal ExportVolume { get; set; }
        public decimal ImportValue { get; set; }
        public decimal ExportValue { get; set; }
    }
    public class WarehouseDiaryItem
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string SeriesCode { get; set; }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        public decimal ImportVolume { get; set; }
        public decimal ExportVolume { get; set; }
        public decimal ImportValue { get; set; }
        public decimal ExportValue { get; set; }
    }
}