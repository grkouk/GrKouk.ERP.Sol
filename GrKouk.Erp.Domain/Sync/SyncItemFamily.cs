using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SyncItemFamily
{
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncUnitOfMeasurement
{
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncVatCategories
{
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(5)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SyncItem
{
    [Required]
    public int BusId { get; set; }
    [MaxLength(30)]
    public string BusCode { get; set; }
    [MaxLength(200)]
    public string Name { get; set; }
    [Required]
    public int ErpId { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}

public class SyncBuyDocument
{
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }

    [Required]
    public DateTime TransDate { get; set; }
    [Required]
    public int SupplierId { get; set; }
    [Required]
    public int RefNumber { get; set; }
  
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}

public class SyncSaleDocument
{
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }

    [Required]
    public DateTime TransDate { get; set; }
    [Required]
    public int CustomerId { get; set; }
    [Required]
    public int RefNumber { get; set; }
  
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}
public class SynchronizationLog
{
    public int Id { get; set; }
    public Guid SyncSessionId { get; set; }
    public DateTime SyncedAt { get; set; }
    [MaxLength(100)]
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    [MaxLength(20)]
    public string OperationType { get; set; }
    [MaxLength(20)]
    public string Source { get; set; }
}