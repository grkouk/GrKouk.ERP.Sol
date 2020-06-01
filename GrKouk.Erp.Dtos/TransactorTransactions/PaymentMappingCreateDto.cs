using System.Collections.Generic;

namespace GrKouk.Erp.Dtos.TransactorTransactions
{
    public class PaymentMappingCreateDto
    {
        public int DocId { get; set; }
        private IList<PaymentMappingLineCreateDto> _paymentMappingLines;
        public virtual IList<PaymentMappingLineCreateDto> PaymentMappingLines
        {
            get => _paymentMappingLines ?? (_paymentMappingLines = new List<PaymentMappingLineCreateDto>());
            set => _paymentMappingLines = value;
        }
    }
    public class PaymentMappingLineCreateDto
    {
        public int ReceiptId { get; set; }
        public decimal AmountUsed { get; set; }

    }
}