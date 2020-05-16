using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.MediaEntities
{
    public class MediaEntry
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string OriginalFileName { get; set; }
        [MaxLength(250)]
        public string MediaFile { get; set; }
    }
}
