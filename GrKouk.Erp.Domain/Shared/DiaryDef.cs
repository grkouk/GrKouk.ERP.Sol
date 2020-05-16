using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Domain.Shared
{
    public class DiaryDef
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DiaryTypeEnum DiaryType { get; set; }
        //[MaxLength(200)]
        //public string Definition { get; set; }
        [MaxLength(200)]
        public string SelectedDocTypes { get; set; }
        [MaxLength(200)]
        public string SelectedMatNatures { get; set; }
        [MaxLength(200)]
        public string SelectedTransTypes { get; set; }

    }
}
