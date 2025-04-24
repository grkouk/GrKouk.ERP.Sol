using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncBuyDocumentListDto
{
    public Guid Id { get; set; }
    public int BusId { get; set; }
    public int ErpId { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
    public DateTime TransDate { get; set; }
 
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
   
    public int RefNumber { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal PayedAmount { get; set; }
    public int BuyDocDefId { get; set; }
   
    public string BuyDocDefName { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyName { get; set; }
    public int CompanyCurrencyId { get; set; }
    public int CompanyId { get; set; }
  
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
    public DateTime SyncedAt { get; set; }
    public string OperationType { get; set; }
    public string Source { get; set; }
    public Guid SyncLogId { get; set; }
    public string SourceChecksum { get; set; }
   
}