using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class MeasureUnit
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }
        [Display(Name = "Decimals")]
        public int? DecimalPlaces { get; set; }
    }
}