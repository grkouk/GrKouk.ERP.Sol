using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;

public class SynchronizationLog
{
    public int Id { get; set; }
    public Guid SyncSessionId { get; set; }
    [MaxLength(15)]
    public string CompanyCode { get; set; }
    public DateTime SyncedAt { get; set; }
    [MaxLength(100)]
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    [MaxLength(20)]
    public string OperationType { get; set; }
    [MaxLength(20)]
    public string Source { get; set; }
}