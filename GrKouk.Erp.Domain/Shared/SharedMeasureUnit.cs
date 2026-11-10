using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

public class SharedMeasureUnit : IHubReceivedStamp
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public DateTime ModifiedAt { get; set; }

    /// <summary>Hub clock when this change was stored; the pull filters by it.</summary>
    public DateTime ReceivedAt { get; set; }
}
