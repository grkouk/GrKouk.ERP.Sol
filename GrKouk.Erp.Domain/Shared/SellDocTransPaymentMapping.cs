using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    public class SellDocTransPaymentMapping
    {
        public int Id { get; set; }
        public int SellDocumentId { get; set; }
        public SellDocument SellDocument { get; set; }

        public int TransactorTransactionId { get; set; }
        public TransactorTransaction TransactorTransaction { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountUsed { get; set; }
    }
}