using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SyncSaleDocument
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [Required]
    public DateTime TransDate { get; set; }
    [Required]
    public int CustomerId { get; set; }
    [Required]
    public int RefNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PayedAmount { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}