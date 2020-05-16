using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.FinDiary
{
    public class FinDiaryTransactionDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία", Prompt = "Ημερομηνία")]
        public DateTime TransactionDate { get; set; }

        [MaxLength(50)]
        [Display(Name = "Αρ.Παρ", Prompt = "Αρ.Παραστατικού")]
        public string ReferenceCode { get; set; }

        public int TransactorId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Συν/νος", Prompt = "Συναλλασόμενος")]
        public string TransactorName { get; set; }

        public int FinTransCategoryId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Κατ.Κιν.", Prompt = "Κατηγορία Κίνησης")]
        public string FinTransCategoryName { get; set; }

        public int CompanyId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Εταιρεία", Prompt = "Εταιρεία")]
        public string CompanyName { get; set; }

        [MaxLength(15)]
        [Display(Name = "Κωδ.Εταιρείας", Prompt = "Κωδ.Εταιρείας")]
        public string CompanyCode { get; set; }

        public int CostCentreId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Κ.Εξόδου", Prompt = "Κ.Εξόδου")]
        public string CostCentreName { get; set; }

        [MaxLength(15)]
        [Display(Name = "Κωδ.Κ.Εξόδου", Prompt = "Κωδ.Κ.Εξόδου")]
        public string CostCentreCode { get; set; }

        public int RevenueCentreId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Κ.Εσόδου", Prompt = "Κ.Εσόδου")]
        public string RevenueCentreName { get; set; }

        [MaxLength(15)]
        [Display(Name = "Κωδ.Κ.Εσόδου", Prompt = "Κωδ.Κ.Εσόδου")]
        public string RevenueCentreCode { get; set; }

        [MaxLength(500)]
        [Display(Name = "Περιγραφή", Prompt = "Περιγραφή")]
        public string Description { get; set; }

        public int Kind { get; set; }

        [Display(Name = "ΦΠΑ", Prompt = "ΦΠΑ")]
        public decimal AmountFpa { get; set; }

        [Display(Name = "Καθαρά", Prompt = "Καθαρά")]
        public decimal AmountNet { get; set; }

        [Display(Name = "Μικτό", Prompt = "Μικτό")]
        public decimal AmountTotal { get; set; }

        public byte[] Timestamp { get; set; }
    }
}