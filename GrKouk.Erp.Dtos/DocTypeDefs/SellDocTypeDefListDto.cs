namespace GrKouk.Erp.Dtos.DocTypeDefs
{
    /// <summary>Row of the Syncfusion index grid of sell document type definitions.</summary>
    public class SellDocTypeDefListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Abbr { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int? TransTransactorDefId { get; set; }
        public string TransTransactorDefName { get; set; }
        public int? TransWarehouseDefId { get; set; }
        public string TransWarehouseDefName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }
}
