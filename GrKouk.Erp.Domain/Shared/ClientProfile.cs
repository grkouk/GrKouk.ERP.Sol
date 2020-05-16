using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Client Machine profile entity
    /// </summary>
    public class ClientProfile
    {
        public int Id { get; set; }
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Serial { get; set; }
        [MaxLength(200)]
        public string Data { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

    }
}