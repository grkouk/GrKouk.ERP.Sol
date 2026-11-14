using System;
using System.Linq;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Erp.Dtos.TransactorTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class SellDocumentMappings
    {
        /// <summary>
        /// Document header from the create request (lines are added by the caller).
        /// Section, fiscal period, doc type and the Trans* amounts are set by the caller.
        /// </summary>
        public static SellDocument ToEntity(this SellDocCreateAjaxDto dto) => new()
        {
            Id = dto.Id,
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SellDocSeriesId = dto.SellDocSeriesId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            PaymentMethodId = dto.PaymentMethodId,
            SalesChannelId = dto.SalesChannelId,
            CompanyId = dto.CompanyId
        };

        /// <summary>
        /// Document header from the modify request, for Attach+Modified (lines and the related
        /// transactions are deleted and rebuilt by the caller). Section, fiscal period, doc type and
        /// the Trans* amounts are set by the caller.
        /// </summary>
        public static SellDocument ToEntity(this SellDocModifyAjaxDto dto) => new()
        {
            Id = dto.Id,
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SellDocSeriesId = dto.SellDocSeriesId,
            SellDocTypeId = dto.SellDocTypeId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            FiscalPeriodId = dto.FiscalPeriodId,
            PaymentMethodId = dto.PaymentMethodId,
            SalesChannelId = dto.SalesChannelId,
            CompanyId = dto.CompanyId,
            Timestamp = dto.Timestamp
        };

        /// <summary>
        /// Transactor transaction for a new document (the document's own and the auto pay-off one).
        /// Series, doc type, section, creator, fiscal period and the Trans* amounts are set by the
        /// caller. The document Id is not copied (AutoMapper copied it; it is always 0 on create).
        /// </summary>
        public static TransactorTransaction ToTransactorTransaction(this SellDocCreateAjaxDto dto) => new()
        {
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId
        };

        /// <summary>Transactor transaction data for a modified document (no Id).</summary>
        public static TransactorTransCreateDto ToTransactorTransCreateDto(this SellDocModifyAjaxDto dto) => new()
        {
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SectionId = dto.SectionId,
            FiscalPeriodId = dto.FiscalPeriodId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId
        };

        /// <summary>
        /// Edit/copy-from view model. Navigations may be missing (null-safe);
        /// unit names/codes on the lines are not available and stay null, as before.
        /// </summary>
        public static SellDocModifyDto ToModifyDto(this SellDocument src) => new()
        {
            Id = src.Id,
            TransDate = src.TransDate,
            TransRefCode = src.TransRefCode,
            SectionId = src.SectionId,
            SectionCode = src.Section?.Code,
            TransactorId = src.TransactorId,
            TransactorName = src.Transactor?.Name,
            FiscalPeriodId = src.FiscalPeriodId,
            FiscalPeriodCode = src.FiscalPeriod?.Code,
            SellDocSeriesId = src.SellDocSeriesId,
            SellDocSeriesCode = src.SellDocSeries?.Code,
            SellDocSeriesName = src.SellDocSeries?.Name,
            SellDocTypeId = src.SellDocTypeId,
            SellDocTypeCode = src.SellDocType?.Code,
            SellDocTypeName = src.SellDocType?.Name,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CompanyCode = src.Company?.Code,
            PaymentMethodId = src.PaymentMethodId,
            SalesChannelId = src.SalesChannelId,
            Timestamp = src.Timestamp,
            SellDocLines = src.SellDocLines.Select(l => l.ToModifyDto()).ToList()
        };

        public static SellDocLineModifyDto ToModifyDto(this SellDocLine src) => new()
        {
            Id = src.Id,
            SellDocumentId = src.SellDocumentId,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItemName = src.WarehouseItem?.Name,
            WarehouseItemNature = src.WarehouseItem == null ? 0 : (int)src.WarehouseItem.WarehouseItemNature,
            TransactionUnitId = src.TransactionUnitId,
            TransactionQuantity = src.TransactionQuantity,
            TransactionUnitFactor = src.TransactionUnitFactor,
            TransUnitPrice = src.TransUnitPrice,
            PrimaryUnitId = src.PrimaryUnitId,
            SecondaryUnitId = src.SecondaryUnitId,
            Factor = src.Factor,
            Quontity1 = src.Quontity1,
            Quontity2 = src.Quontity2,
            FpaRate = src.FpaRate,
            DiscountRate = src.DiscountRate,
            UnitPrice = src.UnitPrice,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            AmountExpenses = src.AmountExpenses,
            Etiology = src.Etiology
        };

        /// <summary>EF-translatable list projection.</summary>
        public static readonly Expression<Func<SellDocument, SellDocListDto>> ToListDto =
            src => new SellDocListDto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransRefCode = src.TransRefCode,
                SectionId = src.SectionId,
                SectionCode = src.Section.Code,
                TransactorId = src.TransactorId,
                TransactorName = src.Transactor.Name,
                SellDocSeriesId = src.SellDocSeriesId,
                SellDocSeriesCode = src.SellDocSeries.Code,
                SellDocSeriesName = src.SellDocSeries.Name,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                TransFpaAmount = src.TransFpaAmount,
                TransNetAmount = src.TransNetAmount,
                TransDiscountAmount = src.TransDiscountAmount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId,
                SalesChannelId = src.SalesChannelId
            };

        /// <summary>
        /// EF-translatable index projection with the paid amount (sum of the payment mappings).
        /// Amounts are in the company currency; convert them for display after loading.
        /// </summary>
        public static readonly Expression<Func<SellDocument, SellDocList2Dto>> ToList2Dto =
            src => new SellDocList2Dto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransRefCode = src.TransRefCode,
                SectionId = src.SectionId,
                SectionCode = src.Section.Code,
                TransactorId = src.TransactorId,
                TransactorName = src.Transactor.Name,
                SellDocSeriesId = src.SellDocSeriesId,
                SellDocSeriesCode = src.SellDocSeries.Code,
                SellDocSeriesName = src.SellDocSeries.Name,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId,
                SalesChannelId = src.SalesChannelId,
                PayedOfAmount = src.PaymentMappings.Sum(t => t.AmountUsed)
            };
    }
}
