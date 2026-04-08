using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

public class SharedItemCode
{
    [Key]
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public int CodeType { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; }

    public Guid MeasureUnitId { get; set; }

    public decimal Quantity { get; set; } = 1;
}
