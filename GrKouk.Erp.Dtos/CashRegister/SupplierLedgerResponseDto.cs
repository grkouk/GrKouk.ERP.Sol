using System;
using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.CashRegister
{
    public class SupplierLedgerResponseDto
    {
        public decimal OpeningBalance { get; set; }
        public DateTime OpeningBalanceDate { get; set; }
        public List<SupplierLedgerRowDto> Rows { get; set; } = new();
    }
}
