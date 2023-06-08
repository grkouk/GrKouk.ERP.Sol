using System;
using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.TransactorTransactions
{
    public class TransactorCrossEntryDto
    {
        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Παραστατικο΄1")]
        public int DocSeries1Id { get; set; }
        [Display(Name = "Παραστατικό 2")]
        public int DocSeries2Id { get; set; }
        [Display(Name = "Συναλλασσόμενος 1")]
        public int Transactor1Id { get; set; }
        [Display(Name = "Συναλλασσόμενος 2")]
        public int Transactor2Id { get; set; }
        [Display(Name = "Χρημ.Λογ.1")]
        public int Cfa1Id { get; set; }
        [Display(Name = "Χρημ.Λογ 2")]
        public int Cfa2Id { get; set; }
        [Display(Name = "Αρ.Παραστατικού")]
        public string RefCode { get; set; }
        [Display(Name = "Αιτιολογία")]
        public string Etiology { get; set; }

        public decimal Amount { get; set; }
        [Display(Name = "Εταιρεία")]
        public int CompanyId { get; set; }
    }
}