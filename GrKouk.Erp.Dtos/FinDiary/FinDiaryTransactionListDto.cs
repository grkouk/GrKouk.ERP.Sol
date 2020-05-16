using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.FinDiary
{
    public class FinDiaryTransactionListDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία", Prompt = "Ημερομηνία")]
        public DateTime TransactionDate { get; set; }

       
        [MaxLength(200)]
        [Display(Name = "Συν/νος", Prompt = "Συναλλασόμενος")]
        public string TransactorName { get; set; }

        [MaxLength(200)]
        [Display(Name = "Κατ.Κιν.", Prompt = "Κατηγορία Κίνησης")]
        public string FinTransCategoryName { get; set; }

        
        [MaxLength(15)]
        [Display(Name = "Κωδ.Κ.Εσόδου", Prompt = "Κωδ.Κ.Εσόδου")]
        public string RevenueCentreCode { get; set; }

        [MaxLength(500)]
        [Display(Name = "Περιγραφή", Prompt = "Περιγραφή")]
        public string Description { get; set; }
     

        [Display(Name = "Μικτό", Prompt = "Μικτό")]
        public decimal AmountTotal { get; set; }
        public string Actions { get; set; }
    }
}
