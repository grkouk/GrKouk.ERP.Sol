using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Erp.Dtos.Media;

namespace GrKouk.Web.ERP.Mappings
{
    public static class MediaEntryMappings
    {
        /// <summary>EF-translatable projection. Url is filled in by the caller.</summary>
        public static readonly Expression<Func<MediaEntry, MediaEntryDto>> ToDto = src => new MediaEntryDto
        {
            Id = src.Id,
            MediaFile = src.MediaFile,
            OriginalFilename = src.OriginalFileName
        };
    }
}
