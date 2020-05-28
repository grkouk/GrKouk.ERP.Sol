using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class AppSetting
    {
        [Display(Name = "Setting Code")]
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }
        [Display(Name = "Setting Value")]
        [MaxLength(500)]
        [Required]
        public string Value { get; set; }
    }
}
