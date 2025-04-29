using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Sync;

public class SyncSupplierListDto
{
    public Guid Id { get; set; }
    public int BusId { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyName { get; set; }
    public int CompanyCurrencyId { get; set; }
    public int CompanyId { get; set; }
    public string BusCode { get; set; }
    public string Name { get; set; }
    public int ErpId { get; set; }
    public string TaxNumber { get; set; }
    public string SourceChecksum { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
    public DateTime SyncedAt { get; set; }
    public string OperationType { get; set; }
    public string Source { get; set; }
    public Guid SyncLogId { get; set; }
}