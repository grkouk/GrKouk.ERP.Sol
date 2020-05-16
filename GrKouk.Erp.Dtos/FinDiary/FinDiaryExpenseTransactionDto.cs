using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.FinDiary
{
    public class FinDiaryExpenseTransactionDto
    {
        public int Id { get; set; }
        //TODO: Αλλαξα τον τρόπο προβολής της ημερομηνίας. Αλλα δεν ειναι σωστό με βάση την γλώσσα προβολής του χρήστη

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyy}") ]
        [Display(Name = "Ημ/νία", Prompt = "Ημερομηνία")]
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

       

        [MaxLength(500)]
        [Display(Name = "Περιγραφή", Prompt = "Περιγραφή")]
        public string Description { get; set; }

        public int Kind { get; set; }

        [Display(Name = "ΦΠΑ", Prompt = "ΦΠΑ")]
        public decimal AmountFpa { get; set; }

        [Display(Name = "Καθαρά", Prompt = "Καθαρά")]
        public decimal AmountNet { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Μικτό", Prompt = "Μικτό")]
        public decimal AmountTotal { get; set; }

        public int CompanyCurrencyId { get; set; }

        public byte[] Timestamp { get; set; }
    }
}