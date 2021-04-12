using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
        public static async Task<(string code,string name)> GetMeasureUnitDetailsAsync(ApiDbContext context,int unitId)
        {
            var itemDetails = await context.MeasureUnits.FirstOrDefaultAsync(p =>
                p.Id==unitId);
            if (itemDetails==null)
            {
                return (string.Empty, string.Empty);
            }
            return (itemDetails.Code,itemDetails.Name);
        }
        public static string EnumToJson<T>()
        {
            var values = Enum.GetValues(typeof(T)).Cast<int>();
            var enumDictionary = values.ToDictionary(value => Enum.GetName(typeof(T), value));
 
            return JsonConvert.SerializeObject(enumDictionary);
        }
    }
}