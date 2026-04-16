using System;

namespace GrKouk.Erp.Dtos.CashRegister;

public class SupplierPaymentUpsertDto
{
    public int? Id { get; set; }
    public string CompanyCode { get; set; }
    public int TransactorId { get; set; }
    public DateTime TransDate { get; set; }
    public string TransRefCode { get; set; }
    public decimal AmountNet { get; set; }
    public decimal AmountFpa { get; set; }
    public decimal AmountDiscount { get; set; }
    public decimal FpaRate { get; set; }
    public decimal DiscountRate { get; set; }
    public string Etiology { get; set; }
    public byte[] Timestamp { get; set; }
}
