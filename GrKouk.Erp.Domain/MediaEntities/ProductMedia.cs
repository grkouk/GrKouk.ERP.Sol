using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.MediaEntities
{
    public class ProductMedia
    {
        public int Id { get; set; }
        public int MediaEntryId { get; set; }
        public MediaEntry MediaEntry { get; set; }
        public int ProductId { get; set; }
        public WarehouseItem Product { get; set; }
    }
}
