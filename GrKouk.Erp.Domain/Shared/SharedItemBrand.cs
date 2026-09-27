using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// Shared item brand, synced between shops exactly like <see cref="SharedItemCategory"/>.
/// Code is deliberately NOT unique here: two shops can hold the same Code under different
/// Ids, and the client resolves that (skip + conflict entry), not the hub.
/// </summary>
public class SharedItemBrand
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
}
