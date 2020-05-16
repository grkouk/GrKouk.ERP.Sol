using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.Diaries
{
    public class DiaryDto
    {
        public string Name { get; set; }
        public DiaryTypeEnum DiaryType { get; set; }
        public string SelectedDocTypes { get; set; }
        public string SelectedMatNatures { get; set; }
        
        public string SelectedTransTypes { get; set; }
    }
}
