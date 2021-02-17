using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Transactors
{
    public class TransactorBigClass
    {
        public int Id { get; set; }
        public string Code { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public int? Zip { get; set; }

        public string PhoneWork { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneFax { get; set; }

        public string EMail { get; set; }

        public int TransactorTypeId { get; set; }
        public string TransactorTypeName { get; set; }
        public string TransactorTypeCode { get; set; }
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }
}
