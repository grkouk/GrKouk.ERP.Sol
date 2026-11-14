using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Dtos.CashFlowTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class CfaTransactionMappings
    {
        /// <summary>Create form pre-filled from an existing transaction ("copy from").</summary>
        public static CfaTransactionCreateDto ToCreateDto(this CashFlowAccountTransaction src) => new()
        {
            TransDate = src.TransDate,
            DocSeriesId = src.DocumentSeriesId,
            DocTypeId = src.DocumentTypeId,
            TransRefCode = src.RefCode,
            CashFlowAccountId = src.CashFlowAccountId,
            SectionId = src.SectionId,
            CreatorId = src.CreatorId,
            CreatorSectionId = src.CreatorSectionId,
            FiscalPeriodId = src.FiscalPeriodId,
            CfaAction = src.CfaAction,
            Amount = src.Amount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId
        };

        /// <summary>New entity from the Create form. Section, doc type, fiscal period and TransAmount are set by the caller.</summary>
        public static CashFlowAccountTransaction ToEntity(this CfaTransactionCreateDto dto) => new()
        {
            TransDate = dto.TransDate,
            DocumentSeriesId = dto.DocSeriesId,
            DocumentTypeId = dto.DocTypeId,
            RefCode = dto.TransRefCode,
            CashFlowAccountId = dto.CashFlowAccountId,
            SectionId = dto.SectionId,
            CreatorId = dto.CreatorId,
            CreatorSectionId = dto.CreatorSectionId,
            FiscalPeriodId = dto.FiscalPeriodId,
            CfaAction = dto.CfaAction,
            Amount = dto.Amount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId
        };

        /// <summary>Edit / Details / Delete form model.</summary>
        public static CfaTransactionModifyDto ToModifyDto(this CashFlowAccountTransaction src) => new()
        {
            Id = src.Id,
            TransDate = src.TransDate,
            DocSeriesId = src.DocumentSeriesId,
            DocTypeId = src.DocumentTypeId,
            TransRefCode = src.RefCode,
            CashFlowAccountId = src.CashFlowAccountId,
            SectionId = src.SectionId,
            CreatorId = src.CreatorId,
            CreatorSectionId = src.CreatorSectionId,
            FiscalPeriodId = src.FiscalPeriodId,
            CfaAction = src.CfaAction,
            Amount = src.Amount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            Timestamp = src.Timestamp
        };

        /// <summary>
        /// Copies the fields the Edit form posts onto a loaded entity. CreatorId and CreatorSectionId
        /// are not posted by the form and stay as they are; section, doc type, fiscal period,
        /// CfaAction and TransAmount are recalculated by the caller.
        /// </summary>
        public static void ApplyTo(this CfaTransactionModifyDto dto, CashFlowAccountTransaction entity)
        {
            entity.TransDate = dto.TransDate;
            entity.DocumentSeriesId = dto.DocSeriesId;
            entity.RefCode = dto.TransRefCode;
            entity.CashFlowAccountId = dto.CashFlowAccountId;
            entity.Amount = dto.Amount;
            entity.Etiology = dto.Etiology;
            entity.CompanyId = dto.CompanyId;
        }

        /// <summary>EF-translatable list projection. CreatorSectionCode is filled by the caller.</summary>
        public static readonly Expression<Func<CashFlowAccountTransaction, CfaTransactionListDto>> ToListDto =
            src => new CfaTransactionListDto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                DocSeriesId = src.DocumentSeriesId,
                DocSeriesName = src.DocumentSeries.Name,
                DocSeriesCode = src.DocumentSeries.Code,
                DocTypeId = src.DocumentTypeId,
                TransRefCode = src.RefCode,
                CashFlowAccountId = src.CashFlowAccountId,
                CashFlowAccountName = src.CashFlowAccount.Name,
                SectionId = src.SectionId,
                SectionCode = src.Section.Code,
                CreatorId = src.CreatorId,
                CreatorSectionId = src.CreatorSectionId,
                FiscalPeriodId = src.FiscalPeriodId,
                CfaAction = src.CfaAction,
                Amount = src.Amount,
                TransAmount = src.TransAmount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId
            };
    }
}
