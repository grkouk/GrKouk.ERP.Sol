using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.CashFlow
{
    public class CashFlowAccountCompanyMapping
    {
        public int CashFlowAccountId { get; set; }
        public CashFlowAccount CashFlowAccount { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
