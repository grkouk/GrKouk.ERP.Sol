using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Domain.Shared
{
    /// <summary>
    /// Χρήση
    /// </summary>
    public class FiscalPeriod : IValidatableObject
    {
        public int Id { get; set; }

        [MaxLength(15)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate > EndDate)
            {
                yield return new ValidationResult("End date must be later than Start date");
            }
        }
    }
}