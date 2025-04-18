using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SyncSupplier
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [MaxLength(30)]
    public string BusCode { get; set; }
    [MaxLength(200)]
    public string Name { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(25)]
    public string TaxNumber { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}