using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// This is used in the old Diary System
    /// Deprecated
    /// The new entity is Profit Center
    /// </summary>
    public class RevenueCentre
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
    
    /// <summary>
    /// Profit Center 
    /// </summary>
    public class ProfitCentre
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
        
        [Required]
        public int CompanyId { get; set; } = 1;
        public virtual Company Company { get; set; }
    }

}