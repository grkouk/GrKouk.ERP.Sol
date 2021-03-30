using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Erp.Domain.MainEntities.Transactors
{
    public class TransactorCompanyMapping
    {
        public int TransactorId { get; set; }
        public Transactor Transactor { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
