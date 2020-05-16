using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class TransWarehouseDocSeriesDef
    {
        public int Id { get; set; }

        [MaxLength(15)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }

        [Display(Name = "Τύπος Παραστατικού")]
        public int TransWarehouseDocTypeDefId { get; set; }
        public TransWarehouseDocTypeDef TransWarehouseDocTypeDef { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}