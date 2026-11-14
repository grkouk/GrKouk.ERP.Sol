using System;
using System.Linq;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Erp.Dtos.SellDocuments;

namespace GrKouk.Web.ERP.Mappings
{
    public static class RecurringDocumentMappings
    {
        /// <summary>Header from the create request (lines are added by the caller; SectionId and DocTypeId are set by the caller).</summary>
        public static RecurringTransDoc ToEntity(this RecurringTransDocCreateAjaxDto dto) => new()
        {
            Id = dto.Id,
            RecurringFrequency = dto.RecurringFrequency,
            RecurringDocType = dto.RecurringDocType,
            NextTransDate = dto.NextTransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SectionId = dto.SectionId,
            DocSeriesId = dto.DocSeriesId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId,
            PaymentMethodId = dto.PaymentMethodId
        };

        /// <summary>
        /// Header from the modify request, for Attach+Modified (the request carries every column
        /// except DocTypeId, which the caller sets; lines are deleted and re-added by the caller).
        /// </summary>
        public static RecurringTransDoc ToEntity(this RecurringTransDocModifyAjaxDto dto) => new()
        {
            Id = dto.Id,
            RecurringFrequency = dto.RecurringFrequency,
            RecurringDocType = dto.RecurringDocType,
            NextTransDate = dto.NextTransDate,
            TransRefCode = dto.TransRefCode,
            TransactorId = dto.TransactorId,
            SectionId = dto.SectionId,
            DocSeriesId = dto.DocSeriesId,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId,
            PaymentMethodId = dto.PaymentMethodId,
            Timestamp = dto.Timestamp
        };

        /// <summary>Edit form model. DocSeriesCode/Name stay null (no navigation), as before.</summary>
        public static RecurringDocModifyDto ToModifyDto(this RecurringTransDoc src) => new()
        {
            Id = src.Id,
            RecurringFrequency = src.RecurringFrequency,
            RecurringDocType = src.RecurringDocType,
            NextTransDate = src.NextTransDate,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            TransactorName = src.Transactor?.Name,
            SectionId = src.SectionId,
            DocSeriesId = src.DocSeriesId,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CompanyCode = src.Company?.Code,
            PaymentMethodId = src.PaymentMethodId,
            Timestamp = src.Timestamp,
            BuyDocLines = src.DocLines.Select(l => l.ToRecurringLineModifyDto()).ToList()
        };

        /// <summary>
        /// "Create recurring from a buy document" model. The recurring fields (frequency, type,
        /// next date) and DocSeriesCode/Name are left empty, as before.
        /// </summary>
        public static RecurringDocModifyDto ToRecurringModifyDto(this BuyDocument src) => new()
        {
            Id = src.Id,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            TransactorName = src.Transactor?.Name,
            SectionId = src.SectionId,
            DocSeriesId = src.BuyDocSeriesId,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CompanyCode = src.Company?.Code,
            PaymentMethodId = src.PaymentMethodId,
            Timestamp = src.Timestamp,
            BuyDocLines = src.BuyDocLines.Select(l => l.ToRecurringLineModifyDto()).ToList()
        };

        /// <summary>"Create recurring from a sell document" model; see the buy overload.</summary>
        public static RecurringDocModifyDto ToRecurringModifyDto(this SellDocument src) => new()
        {
            Id = src.Id,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            TransactorName = src.Transactor?.Name,
            SectionId = src.SectionId,
            DocSeriesId = src.SellDocSeriesId,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            CompanyCode = src.Company?.Code,
            PaymentMethodId = src.PaymentMethodId,
            Timestamp = src.Timestamp,
            BuyDocLines = src.SellDocLines.Select(l => l.ToRecurringLineModifyDto()).ToList()
        };

        public static RecurringDocLineModifyDto ToRecurringLineModifyDto(this RecurringTransDocLine src) => new()
        {
            Id = src.Id,
            RecurringTransDocId = src.RecurringTransDocId,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItemName = src.WarehouseItem?.Name,
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
            Etiology = src.Etiology
        };

        public static RecurringDocLineModifyDto ToRecurringLineModifyDto(this BuyDocLine src) => new()
        {
            Id = src.Id,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItemName = src.WarehouseItem?.Name,
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
            Etiology = src.Etiology
        };

        public static RecurringDocLineModifyDto ToRecurringLineModifyDto(this SellDocLine src) => new()
        {
            Id = src.Id,
            WarehouseItemId = src.WarehouseItemId,
            WarehouseItemName = src.WarehouseItem?.Name,
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
            Etiology = src.Etiology
        };

        /// <summary>Buy document request generated from a recurring definition (dated NextTransDate).</summary>
        public static BuyDocCreateAjaxDto ToBuyDocCreateAjaxDto(this RecurringTransDoc src) => new()
        {
            TransDate = src.NextTransDate,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            BuyDocSeriesId = src.DocSeriesId,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            PaymentMethodId = src.PaymentMethodId,
            CompanyId = src.CompanyId,
            BuyDocLines = src.DocLines.Select(l => new BuyDocLineAjaxDto
            {
                WarehouseItemId = l.WarehouseItemId,
                Q1 = l.Quontity1,
                Q2 = l.Quontity2,
                Price = l.UnitPrice,
                AmountDiscount = l.AmountDiscount,
                DiscountRate = (float)l.DiscountRate,
                MainUnitId = l.PrimaryUnitId,
                SecUnitId = l.SecondaryUnitId,
                Factor = l.Factor,
                FpaRate = (float)l.FpaRate
            }).ToList()
        };

        /// <summary>Sell document request generated from a recurring definition (dated NextTransDate).</summary>
        public static SellDocCreateAjaxDto ToSellDocCreateAjaxDto(this RecurringTransDoc src) => new()
        {
            TransDate = src.NextTransDate,
            TransRefCode = src.TransRefCode,
            TransactorId = src.TransactorId,
            SellDocSeriesId = src.DocSeriesId,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            Etiology = src.Etiology,
            PaymentMethodId = src.PaymentMethodId,
            CompanyId = src.CompanyId,
            SellDocLines = src.DocLines.Select(l => new SellDocLineAjaxDto
            {
                WarehouseItemId = l.WarehouseItemId,
                Q1 = l.Quontity1,
                Q2 = l.Quontity2,
                Price = l.UnitPrice,
                AmountDiscount = l.AmountDiscount,
                DiscountRate = (float)l.DiscountRate,
                MainUnitId = l.PrimaryUnitId,
                SecUnitId = l.SecondaryUnitId,
                Factor = l.Factor,
                FpaRate = (float)l.FpaRate
            }).ToList()
        };

        /// <summary>
        /// EF-translatable list projection. SectionCode, DocSeriesCode/Name and RecurringDocTypeName
        /// are filled by the caller (there are no navigations for them).
        /// </summary>
        public static readonly Expression<Func<RecurringTransDoc, RecurringDocListDto>> ToListDto =
            src => new RecurringDocListDto
            {
                Id = src.Id,
                RecurringFrequency = src.RecurringFrequency,
                RecurringDocType = src.RecurringDocType,
                NextTransDate = src.NextTransDate,
                TransRefCode = src.TransRefCode,
                SectionId = src.SectionId,
                TransactorId = src.TransactorId,
                TransactorName = src.Transactor.Name,
                DocSeriesId = src.DocSeriesId,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId
            };
    }
}
