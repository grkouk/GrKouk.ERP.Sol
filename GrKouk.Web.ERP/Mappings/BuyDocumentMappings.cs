using System;
using System.Linq;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.TransactorTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class BuyDocumentMappings
    {
        /// <summary>
        /// Document header from the create request (lines are added by the caller).
        /// Section, fiscal period, doc type and the Trans* amounts are set by the caller.
        /// </summary>
        public static BuyDocument ToEntity(this BuyDocCreateAjaxDto dto) => new()
        {
            Id = dto.Id,
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            BuyDocSeriesId = dto.BuyDocSeriesId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId,
            PaymentMethodId = dto.PaymentMethodId
        };

        /// <summary>
        /// Document header from the modify request, for Attach+Modified (lines and the related
        /// transactions are deleted and rebuilt by the caller). Section, fiscal period, doc type and
        /// the Trans* amounts are set by the caller.
        /// </summary>
        public static BuyDocument ToEntity(this BuyDocModifyAjaxDto dto) => new()
        {
            Id = dto.Id,
            TransDate = dto.TransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            BuyDocSeriesId = dto.BuyDocSeriesId,
            BuyDocTypeId = dto.BuyDocTypeId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            FiscalPeriodId = dto.FiscalPeriodId,
            PaymentMethodId = dto.PaymentMethodId,
            CompanyId = dto.CompanyId,
            Timestamp = dto.Timestamp
        };

        /// <summary>
        /// Transactor transaction for a new document (the document's own and the auto pay-off one).
        /// Series, doc type, section, creator, fiscal period and the Trans* amounts are set by the
        /// caller. The document Id is not copied (AutoMapper copied it; it is always 0 on create).
        /// </summary>
        public static TransactorTransaction ToTransactorTransaction(this BuyDocCreateAjaxDto dto) => new()
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
        public static TransactorTransCreateDto ToTransactorTransCreateDto(this BuyDocModifyAjaxDto dto) => new()
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
        /// Edit/Details/Delete/copy-from view model. Navigations may be missing (null-safe);
        /// unit names/codes on the lines are not available and stay null, as before.
        /// </summary>
        public static BuyDocModifyDto ToModifyDto(this BuyDocument src) => new()
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
            BuyDocSeriesId = src.BuyDocSeriesId,
            BuyDocSeriesCode = src.BuyDocSeries?.Code,
            BuyDocSeriesName = src.BuyDocSeries?.Name,
            BuyDocTypeId = src.BuyDocTypeId,
            BuyDocTypeCode = src.BuyDocType?.Code,
            BuyDocTypeName = src.BuyDocType?.Name,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CompanyCode = src.Company?.Code,
            PaymentMethodId = src.PaymentMethodId,
            Timestamp = src.Timestamp,
            BuyDocLines = src.BuyDocLines.Select(l => l.ToModifyDto()).ToList()
        };

        public static BuyDocLineModifyDto ToModifyDto(this BuyDocLine src) => new()
        {
            Id = src.Id,
            BuyDocumentId = src.BuyDocumentId,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItemName = src.WarehouseItem?.Name,
            PrimaryUnitId = src.PrimaryUnitId,
            SecondaryUnitId = src.SecondaryUnitId,
            TransactionUnitId = src.TransactionUnitId,
            TransactionQuantity = src.TransactionQuantity,
            TransactionUnitFactor = src.TransactionUnitFactor,
            Factor = src.Factor,
            Quontity1 = src.Quontity1,
            Quontity2 = src.Quontity2,
            FpaRate = src.FpaRate,
            DiscountRate = src.DiscountRate,
            UnitPrice = src.UnitPrice,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            TransUnitPrice = src.TransUnitPrice,
            AmountExpenses = src.AmountExpenses,
            Etiology = src.Etiology
        };

        /// <summary>EF-translatable list projection.</summary>
        public static readonly Expression<Func<BuyDocument, BuyDocListDto>> ToListDto =
            src => new BuyDocListDto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransRefCode = src.TransRefCode,
                SectionId = src.SectionId,
                SectionCode = src.Section.Code,
                TransactorId = src.TransactorId,
                TransactorName = src.Transactor.Name,
                BuyDocSeriesId = src.BuyDocSeriesId,
                BuyDocSeriesCode = src.BuyDocSeries.Code,
                BuyDocSeriesName = src.BuyDocSeries.Name,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                TransFpaAmount = src.TransFpaAmount,
                TransNetAmount = src.TransNetAmount,
                TransDiscountAmount = src.TransDiscountAmount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId
            };
    }
}
