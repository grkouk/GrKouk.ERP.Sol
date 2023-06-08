using System;

namespace GrKouk.Web.ERP.Helpers;

public class CfaFinancialDataResponse
{
    public decimal SumOfDeposits { get; set; }
    public decimal SumOfWithdraws { get; set; }
    public decimal SumOfDifference { get; set; }
    public DateTime LastDebitDate { get; set; }
    public DateTime LastCreditDate { get; set; }
}