using System.ComponentModel.DataAnnotations;

namespace GrKouk.Erp.Dtos.Transactors
{
    public class TransactorListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        
        public string Name { get; set; }
        
        public string EMail { get; set; }
        [Display(Name = "Transactor Type")]
        public string TransactorTypeName { get; set; }

        public string TransactorTypeCode { get; set; }
        public string CompanyCode { get; set; }
       
    }
}
