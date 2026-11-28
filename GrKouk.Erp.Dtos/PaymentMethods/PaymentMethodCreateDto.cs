using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.PaymentMethods
{
    /// <summary>The Create form of a payment method: every field of the entity except Id and the dates.</summary>
    public class PaymentMethodCreateDto
    {
        [MaxLength(20)]
        [Required]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        public bool Active { get; set; } = true;

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
    }
}
