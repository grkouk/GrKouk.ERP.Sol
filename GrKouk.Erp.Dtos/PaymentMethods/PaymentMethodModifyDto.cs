using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.PaymentMethods
{
    /// <summary>
    /// The Edit form of a payment method. CreatedAt and ModifiedAt are shown, not posted:
    /// the mapping never copies them back to the entity.
    /// </summary>
    public class PaymentMethodModifyDto
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        public bool Active { get; set; }

        [Display(Name = "Days Overdue")]
        [Range(0, 3650)]
        public int DaysOverdue { get; set; }

        [Display(Name = "Auto Payoff")]
        public SeriesAutoPayoffEnum AutoPayoffWay { get; set; }

        [Display(Name = "Payoff Series")]
        public int? PayoffSeriesId { get; set; }

        [Display(Name = "Cash Flow Account")]
        public int CfAccountId { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Modified")]
        public DateTime ModifiedAt { get; set; }
    }
}
