using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class FinancialMovement
    {

        public int Id { get; set; }

        [MaxLength(15)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        /// <summary>
        /// + Αυξάνει
        /// - Μειώνει (αντιστροφή προσήμου κίνησης)
        /// ???????
        /// -+ Αυξάνει αρνητικά (αντιστροφή προσήμου κίνησης)
        /// -- Μειώνει αρνητικά (αντιστροφή προσήμου κίνησης)
        /// = Αμετάβλητο
        /// </summary>
        public string Action { get; set; }
    }
}