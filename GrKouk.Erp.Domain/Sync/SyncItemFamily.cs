using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SyncItemFamily
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    
    [MaxLength(15)]
    public string CompanyCode { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncUnitOfMeasurement
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncVatCategories
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [MaxLength(5)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncItem
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [MaxLength(30)]
    public string BusCode { get; set; }
    [MaxLength(200)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}