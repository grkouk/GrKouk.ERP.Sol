using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class CostCentre
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
    }
   
}