using System.Linq;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Dtos.PaymentMethods;
using GrKouk.Web.ERP.Data;

namespace GrKouk.Web.ERP.Mappings
{
    public static class PaymentMethodMappings
    {
        /// <summary>New entity from the Create form. The dates are set by the caller.</summary>
        public static PaymentMethod ToEntity(this PaymentMethodCreateDto dto) => new()
        {
            Code = dto.Code,
            Name = dto.Name,
            Active = dto.Active,
            DaysOverdue = dto.DaysOverdue,
            AutoPayoffWay = dto.AutoPayoffWay,
            PayoffSeriesId = dto.PayoffSeriesId,
            CfAccountId = dto.CfAccountId,
            CompanyId = dto.CompanyId
        };

        /// <summary>Edit form model.</summary>
        public static PaymentMethodModifyDto ToModifyDto(this PaymentMethod src) => new()
        {
            Id = src.Id,
            Code = src.Code,
            Name = src.Name,
            Active = src.Active,
            DaysOverdue = src.DaysOverdue,
            AutoPayoffWay = src.AutoPayoffWay,
            PayoffSeriesId = src.PayoffSeriesId,
            CfAccountId = src.CfAccountId,
            CompanyId = src.CompanyId,
            CreatedAt = src.CreatedAt,
            ModifiedAt = src.ModifiedAt
        };

        /// <summary>
        /// Copies the editable fields onto a loaded entity. Id and the dates are deliberately not
        /// touched (the Edit form only shows the dates; ModifiedAt is set by the page).
        /// </summary>
        public static void ApplyTo(this PaymentMethodModifyDto dto, PaymentMethod entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Active = dto.Active;
            entity.DaysOverdue = dto.DaysOverdue;
            entity.AutoPayoffWay = dto.AutoPayoffWay;
            entity.PayoffSeriesId = dto.PayoffSeriesId;
            entity.CfAccountId = dto.CfAccountId;
            entity.CompanyId = dto.CompanyId;
        }

        /// <summary>
        /// EF-translatable list rows. PaymentMethod has no navigation properties, so the cash flow
        /// account, payoff series and company are left joined (all optional: 0/null means none).
        /// </summary>
        public static IQueryable<PaymentMethodListDto> ToListQuery(ApiDbContext db) =>
            from p in db.PaymentMethods
            join cfa in db.CashFlowAccounts on p.CfAccountId equals cfa.Id into cfas
            from cfa in cfas.DefaultIfEmpty()
            join s in db.TransTransactorDocSeriesDefs on p.PayoffSeriesId equals (int?)s.Id into series
            from s in series.DefaultIfEmpty()
            join c in db.Companies on p.CompanyId equals c.Id into companies
            from c in companies.DefaultIfEmpty()
            select new PaymentMethodListDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Active = p.Active,
                DaysOverdue = p.DaysOverdue,
                AutoPayoffWay = p.AutoPayoffWay,
                PayoffSeriesId = p.PayoffSeriesId,
                PayoffSeriesName = s.Name,
                CfAccountId = p.CfAccountId,
                CfAccountName = cfa.Name,
                CompanyId = p.CompanyId,
                CompanyCode = c.Code
            };
    }
}
