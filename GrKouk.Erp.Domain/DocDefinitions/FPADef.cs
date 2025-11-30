using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class FpaDef
    {
        public int Id { get; set; }

        [MaxLength(15)] [Required] public string Code { get; set; }

        [MaxLength(200)] [Required] public string Name { get; set; }

        public Single Rate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } 
        public int CompanyId { get; set; }  
    }
}