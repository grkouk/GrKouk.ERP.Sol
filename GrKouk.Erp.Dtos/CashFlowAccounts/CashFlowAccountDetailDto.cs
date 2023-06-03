namespace GrKouk.Erp.Dtos.CashFlowAccounts;

public class CashFlowAccountDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string TransactorTypeName { get; set; }
    public string TransactorTypeCode { get; set; }
    public string Companies { get; set; }
}