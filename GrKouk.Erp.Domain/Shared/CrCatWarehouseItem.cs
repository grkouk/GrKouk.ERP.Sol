using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Warehouse Item Cash Category Cross Table
    /// </summary>
    public class CrCatWarehouseItem 
    {
        public int Id { get; set; }
        [Display(Name = "Client Profile")]
        public int ClientProfileId { get; set; }
        public virtual ClientProfile ClientProfile { get; set; }
        [Display(Name = "Cash Reg. Category")]
        public int CashRegCategoryId { get; set; }
        public virtual CashRegCategory CashRegCategory { get; set; }
        [Display(Name = "Warehouse Item")]
        public int WarehouseItemId { get; set; }    
        public virtual WarehouseItem WarehouseItem { get; set; }

    }
}
