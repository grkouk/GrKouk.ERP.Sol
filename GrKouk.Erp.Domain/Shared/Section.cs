using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Κυκλώματα
    /// </summary>
    public class Section
    {
        public int Id { get; set; }

        [MaxLength(25)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
        [MaxLength(50)]
        [Required]
        public string SystemName { get; set; }
    }
}