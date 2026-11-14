using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class TransactorTransactionMappings
    {
        /// <summary>Create form pre-filled from an existing transaction ("copy from").</summary>
        public static TransactorTransCreateDto ToCreateDto(this TransactorTransaction src) => new()
        {
            TransDate = src.TransDate,
            TransTransactorDocSeriesId = src.TransTransactorDocSeriesId,
            TransTransactorDocTypeId = src.TransTransactorDocTypeId,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            SectionId = src.SectionId,
            CreatorId = src.CreatorId,
            CreatorSectionId = src.CreatorSectionId,
            FiscalPeriodId = src.FiscalPeriodId,
            FinancialAction = src.FinancialAction,
            FpaRate = src.FpaRate,
            DiscountRate = src.DiscountRate,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CfAccountId = src.CfAccountId
        };

        /// <summary>
        /// New entity from a create DTO. The Trans* amounts are set by the caller
        /// (ActionHandlers.TransactorFinAction).
        /// </summary>
        public static TransactorTransaction ToEntity(this TransactorTransCreateDto dto) => new()
        {
            TransDate = dto.TransDate,
            TransTransactorDocSeriesId = dto.TransTransactorDocSeriesId,
            TransTransactorDocTypeId = dto.TransTransactorDocTypeId,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SectionId = dto.SectionId,
            CreatorId = dto.CreatorId,
            CreatorSectionId = dto.CreatorSectionId,
            FiscalPeriodId = dto.FiscalPeriodId,
            FinancialAction = dto.FinancialAction,
            FpaRate = dto.FpaRate,
            DiscountRate = dto.DiscountRate,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId,
            CfAccountId = dto.CfAccountId
        };

        /// <summary>Edit, Details and Delete form model.</summary>
        public static TransactorTransModifyDto ToModifyDto(this TransactorTransaction src) => new()
        {
            Id = src.Id,
            TransDate = src.TransDate,
            TransTransactorDocSeriesId = src.TransTransactorDocSeriesId,
            TransTransactorDocTypeId = src.TransTransactorDocTypeId,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            SectionId = src.SectionId,
            CreatorId = src.CreatorId,
            CreatorSectionId = src.CreatorSectionId,
            FiscalPeriodId = src.FiscalPeriodId,
            FinancialAction = src.FinancialAction,
            FpaRate = src.FpaRate,
            DiscountRate = src.DiscountRate,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            Timestamp = src.Timestamp,
            CfAccountId = src.CfAccountId
        };

        /// <summary>
        /// EF-translatable list projection. CreatorSectionCode is left null (there is no navigation
        /// for it); callers that show it fill it in.
        /// </summary>
        public static readonly Expression<Func<TransactorTransaction, TransactorTransListDto>> ToListDto =
            src => new TransactorTransListDto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransTransactorDocSeriesId = src.TransTransactorDocSeriesId,
                TransTransactorDocSeriesName = src.TransTransactorDocSeries.Name,
                TransTransactorDocSeriesCode = src.TransTransactorDocSeries.Code,
                TransTransactorDocTypeId = src.TransTransactorDocTypeId,
                TransRefCode = src.TransRefCode,
                TransactorId = src.TransactorId,
                TransactorName = src.Transactor.Name,
                SectionId = src.SectionId,
                SectionCode = src.Section.Code,
                CreatorId = src.CreatorId,
                CreatorSectionId = src.CreatorSectionId,
                FiscalPeriodId = src.FiscalPeriodId,
                FinancialAction = src.FinancialAction,
                FpaRate = src.FpaRate,
                DiscountRate = src.DiscountRate,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                TransFpaAmount = src.TransFpaAmount,
                TransNetAmount = src.TransNetAmount,
                TransDiscountAmount = src.TransDiscountAmount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId,
                CfAccountId = src.CfAccountId
            };
    }
}
