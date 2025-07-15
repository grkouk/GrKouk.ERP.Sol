using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Warehouses
{
    public class WarehouseCreateDto
    {
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }
       
        public string SelectedCompanies { get; set; }
    }
}
