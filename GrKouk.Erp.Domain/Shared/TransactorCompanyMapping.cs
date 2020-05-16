namespace GrKouk.Erp.Domain.Shared
{
    public class TransactorCompanyMapping
    {
        public int TransactorId { get; set; }
        public Transactor Transactor { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
