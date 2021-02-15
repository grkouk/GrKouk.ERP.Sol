using System;
using System.Collections.Generic;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Web.ERP.Helpers
{
    public class TransCompMappingEq : IEqualityComparer<TransactorCompanyMapping>
    {
        public bool Equals(TransactorCompanyMapping x, TransactorCompanyMapping y)
        {
            if (x.CompanyId == y.CompanyId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(TransactorCompanyMapping obj)
        {
            throw new NotImplementedException();
        }
    }
}
