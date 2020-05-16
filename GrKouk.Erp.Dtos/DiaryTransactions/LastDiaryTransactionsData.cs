namespace GrKouk.Erp.Dtos.DiaryTransactions
{
    /// <summary>
    /// Παρέχει τα στοιχεία της τελευταίας κίνησης ενός Transactor 
    /// δηλαδή επιστρέφει την καταγορία το κέντρο εξόδου κλπ
    /// </summary>
    public class LastDiaryTransactionsData
    {
        public int CategoryId { get; set; }
        public int CostCentreId { get; set; }
        public int CompanyId { get; set; }
        public int RevenueCentreId { get; set; }
    }
}
