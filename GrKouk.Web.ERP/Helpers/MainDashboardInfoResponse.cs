namespace GrKouk.Web.ERP.Helpers
{
    public class MainDashboardInfoResponse
    {
        public decimal SumOfMaterialBuys { get; set; }
        public decimal SumOfServiceBuys { get; set; }
        public decimal SumOfExpenseBuys { get; set; }
        
        public decimal SumOfMaterialSales { get; set; }
        public decimal SumOfServiceSales { get; set; }
        public decimal SumOfIncomeSales { get; set; }
        public decimal RequestedCodeSum { get; set; }
        public string RequestedCodeToCompute { get; set; }
    }
}