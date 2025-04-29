using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Sync;

public class SynchronizationLogListDto
{
    public Guid Id { get; set; }
   
    public Guid SyncSessionId { get; set; }
    
    public string CompanyCode { get; set; }
    
    public string CompanyName { get; set; }
    public int CompanyCurrencyId { get; set; }
    public int CompanyId { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
    public DateTime SyncedAt { get; set; }
    public string EntityName { get; set; }
    public Guid EntityId { get; set; }
    public string OperationType { get; set; }
    public string Source { get; set; }
}