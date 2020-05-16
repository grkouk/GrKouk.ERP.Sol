using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    public class BuyDocTransPaymentMapping
    {
        public int Id { get; set; }
        //[ForeignKey("BuyDocument")]
        public int BuyDocumentId { get; set; } 
        public BuyDocument BuyDocument { get; set; }

        public int TransactorTransactionId { get; set; }
        public TransactorTransaction TransactorTransaction { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal AmountUsed { get; set; }
    }
}
