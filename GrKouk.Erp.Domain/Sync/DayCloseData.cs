using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Sync;
//TODO: Maybe I dont need this

public class DayCloseData
{
    public Guid Id { get; set; }
    [Required]
    public DateTime TransDate { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "ZNumber must be a non-negative value")]
    public int ZNumber { get; set; }
    
    public decimal AmountCash {get; set;}
    public decimal AmountCards {get; set;}
    public decimal AmountStar {get; set;}
    [MaxLength(15)]
    public string CompanyCode { get; set; }
}