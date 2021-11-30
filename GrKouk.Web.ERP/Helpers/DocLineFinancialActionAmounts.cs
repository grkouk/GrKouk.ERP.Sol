using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Web.ERP.Helpers
{
    /// <summary>
    /// Class used for calculating transaction amounts for Doc Line rows based on financial action
    /// </summary>
    public class DocLineFinancialActionAmounts
    {

        public decimal AmountFpa { get; set; }

        public decimal AmountNet { get; set; }

        public decimal AmountDiscount { get; set; }

        public decimal AmountExpenses { get; set; }


        public decimal TransFpaAmount { get; set; }

        public decimal TransNetAmount { get; set; }

        public decimal TransDiscountAmount { get; set; }

        public decimal TransExpensesAmount { get; set; }
    }
}
