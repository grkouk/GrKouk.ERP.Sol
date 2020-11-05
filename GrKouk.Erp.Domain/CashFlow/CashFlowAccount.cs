using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using GrKouk.Erp.Domain.DocDefinitions;

namespace GrKouk.Erp.Domain.CashFlow
{
    public class CashFlowAccount
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
    }
}
