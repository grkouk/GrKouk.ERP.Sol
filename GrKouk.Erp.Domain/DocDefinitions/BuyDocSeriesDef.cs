using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class BuyDocSeriesDef
    {
        public int Id { get; set; }

        [MaxLength(15)]
        [Required]
        public string Code { get; set; }

        [Display(Name = "Συντόμευση", Prompt = "Συντόμευση της σειράς")]
        [MaxLength(20)]
        public string Abbr { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [Display(Name = "Ενεργό")]
        [DefaultValue(true)]
        public bool Active { get; set; }

        [Display(Name = "Τύπος Παραστατικού")]
        public int BuyDocTypeDefId { get; set; }
        public virtual BuyDocTypeDef BuyDocTypeDef { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        /// <summary>
        /// Τρόπος αυτόματης εξόφλησης σειράς παραστατικού
        /// </summary>
        public SeriesAutoPayoffEnum AutoPayoffWay { get; set; }
        public int? PayoffSeriesId { get; set; }


    }
}
