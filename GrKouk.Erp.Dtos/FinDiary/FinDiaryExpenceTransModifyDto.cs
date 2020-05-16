using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.FinDiary
{
    public class FinDiaryExpenceTransModifyDto
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία", Prompt = "Ημερομηνία")]
        public DateTime TransactionDate { get; set; }
        [MaxLength(50)]
        [Display(Name = "Αρ.Παρ", Prompt = "Αρ.Παραστατικού")]
        public string ReferenceCode { get; set; }

        [Display(Name = "Συναλλασσόμενος", Prompt = "Συναλλασσόμενος")]
        public int TransactorId { get; set; }
        [Display(Name = "Κατηγορία", Prompt = "Κατηγορία")]
        public int FinTransCategoryId { get; set; }
        [Display(Name = "Εταιρεία", Prompt = "Εταιρεία")]
        public int CompanyId { get; set; }
        [Display(Name = "Κατ.Εξόδου", Prompt = "Κατ.Εσόδου")]
        public int CostCentreId { get; set; }
        public int RevenueCentreId { get; set; }
        [Display(Name = "Περιγραφή", Prompt = "Περιγραφή")]
        [MaxLength(500)]
        public string Description { get; set; }
        public int Kind { get; set; }
        [Display(Name = "ΦΠΑ", Prompt = "ΦΠΑ")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Καθ.Ποσό", Prompt = "Καθ.Ποσό")]
        public decimal AmountNet { get; set; }
        public byte[] Timestamp { get; set; }
    }
}
