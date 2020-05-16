using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class WarehouseItemCode   
    {
        public WarehouseItemCodeTypeEnum CodeType { get; set; }
        [MaxLength(30)]
        public string Code { get; set; }
        public int TransactorId { get; set; }
        public int WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
        public WarehouseItemCodeUsedUnitEnum CodeUsedUnit { get; set; }
        public double ToMainUnitRate { get; set; }

    }
}