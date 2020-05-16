using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Cash register Category
    /// </summary>
   public class CashRegCategory
    {
        public int Id { get; set; }
      
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
}
