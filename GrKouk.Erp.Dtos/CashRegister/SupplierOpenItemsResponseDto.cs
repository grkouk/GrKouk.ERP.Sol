using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.CashRegister
{
    /// <summary>
    /// Open-item payables for a supplier, pooled across the selected branches and
    /// FIFO-allocated as one account. Current = due on/before asOf (what is owed now),
    /// Future = not yet due. Residual = combined ledger balance minus the sum of open
    /// items (opening balances / manual credits not tied to a BuyDocument). Approximate
    /// is true when any due date fell back to the document date (missing credit term)
    /// or the residual is non-zero.
    /// </summary>
    public class SupplierOpenItemsResponseDto
    {
        public List<SupplierOpenItemDto> Current { get; set; } = new();
        public List<SupplierOpenItemDto> Future { get; set; } = new();
        public decimal CurrentTotal { get; set; }
        public decimal FutureTotal { get; set; }
        public decimal Residual { get; set; }
        public decimal CombinedBalance { get; set; }
        public bool Approximate { get; set; }
    }
}
