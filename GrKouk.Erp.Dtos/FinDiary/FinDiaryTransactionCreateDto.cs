using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.FinDiary
{
    public class FinDiaryTransactionCreateDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία", Prompt = "Ημερομηνία")]
        public DateTime TransactionDate { get; set; }

        [MaxLength(50)]
        [Display(Name = "Αρ.Παραστατικού", Prompt = "Αρ.Παραστατικού")]
        public string ReferenceCode { get; set; }

        public int TransactorId { get; set; }

        public int FinTransCategoryId { get; set; }
        public int CompanyId { get; set; }
        public int CostCentreId { get; set; }
        public int RevenueCentreId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Περιγραφή", Prompt = "Περιγραφή")]
        public string Description { get; set; }

        public int Kind { get; set; }
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }
    }
}