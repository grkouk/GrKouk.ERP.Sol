using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    public class Currency
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required]
        public string Code { get; set; }
        [MaxLength(20)]
        [Display(Name = "Display Locale")]
        public string DisplayLocale { get; set; }
        [MaxLength(200)]
        [Display(Name = "Currency Name")]
        [Required]
        public string Name { get; set; }

        private ICollection<ExchangeRate> _rates;
        public ICollection<ExchangeRate> Rates
        {
            get => _rates ??= new List<ExchangeRate>();
            set => _rates = value;
        }
    }
}
