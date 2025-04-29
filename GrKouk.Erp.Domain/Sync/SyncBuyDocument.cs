using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SyncBuyDocument
{
    public Guid Id { get; set; }
    [Required]
    public int BusId { get; set; }
    [Required]
    public int ErpId { get; set; }
   
    public int BuyDocDefId { get; set; }
    public string? BuyDocDefName { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    [Required]
    public DateTime TransDate { get; set; }
    [Required]
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    [Required]
    public int RefNumber { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal PayedAmount { get; set; }
    [Required]
    [MaxLength(32)]
    public string SourceChecksum { get; set; }
}