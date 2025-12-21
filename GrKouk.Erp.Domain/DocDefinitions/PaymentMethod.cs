using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Domain.DocDefinitions
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public int DaysOverdue { get; set; }
        public SeriesAutoPayoffEnum AutoPayoffWay { get; set; }
        public int? PayoffSeriesId { get; set; }
        public int CfAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } 
        public int CompanyId { get; set; }  
    }

    //public class FinancialAccount
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }

    //}
}
