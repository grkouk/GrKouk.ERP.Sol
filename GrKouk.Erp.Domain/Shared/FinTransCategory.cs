using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Κατηγορία κίνησης ημερολογίου ή οικονομικής
    /// </summary>
    public class FinTransCategory
    {
        public int Id { get; set; }
        /// <summary>
        /// Κωδικός της κατηγορίας
        /// </summary>
        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
}
