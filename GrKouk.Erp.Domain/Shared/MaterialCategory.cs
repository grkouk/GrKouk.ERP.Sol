using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class MaterialCategory
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        public int CompanyId { get; set; } = 1;
        public virtual Company Company { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } 
    }
}