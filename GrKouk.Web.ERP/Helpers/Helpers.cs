using System;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers
{
    public static class HelperFunctions    
    {
        public static async Task<FiscalPeriod> GetFiscalPeriod(ApiDbContext context,DateTime lookupDate)
        {
            var fiscalPeriod = await context.FiscalPeriods.FirstOrDefaultAsync(p =>
                lookupDate >= p.StartDate && lookupDate <= p.EndDate);
            return fiscalPeriod;
        }
    }
}