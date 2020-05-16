using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime ClosingDate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
