using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class TransTransactorDocTypeDef
    {
        public int Id { get; set; }

        [MaxLength(15)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Display(Name = "Ενεργό")]
        public bool Active { get; set; }
        /// <summary>
        /// Κωδικός κίνησης πελάτη που δημιουργεί αυτός ο τύπος παραστατικού
        /// </summary>
        [Display(Name = "Κίνηση Συναλλασομένου")]
        public int? TransTransactorDefId { get; set; }
        public TransTransactorDef TransTransactorDef { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        [Display(Name = "Default Section")]
        public int SectionId { get; set; }
    }
}