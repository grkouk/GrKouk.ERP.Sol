using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseTransactions;

namespace GrKouk.Web.ERP.Mappings
{
    public static class WarehouseTransactionMappings
    {
        /// <summary>
        /// New entity from the Create form. Section, doc type, inventory actions, TransactionType
        /// and the Trans* amounts are set by the caller.
        /// </summary>
        public static WarehouseTransaction ToEntity(this WarehouseTransCreateDto dto) => new()
        {
            TransDate = dto.TransDate,
            TransWarehouseDocSeriesId = dto.TransWarehouseDocSeriesId,
            TransRefCode = dto.TransRefCode,
            WarehouseItemId = dto.WarehouseItemId,
            FiscalPeriodId = dto.FiscalPeriodId,
            TransactionType = dto.TransactionType,
            InventoryAction = dto.InventoryAction,
            InventoryValueAction = dto.InventoryValueAction,
            SectionId = dto.SectionId,
            PrimaryUnitId = dto.PrimaryUnitId,
            SecondaryUnitId = dto.SecondaryUnitId,
            UnitFactor = dto.UnitFactor,
            Quontity1 = dto.Quontity1,
            Quontity2 = dto.Quontity2,
            FpaRate = dto.FpaRate,
            DiscountRate = dto.DiscountRate,
            UnitPrice = dto.UnitPrice,
            UnitExpenses = dto.UnitExpenses,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet,
            AmountDiscount = dto.AmountDiscount,
            AmountExpenses = dto.AmountExpenses,
            Etiology = dto.Etiology,
            CompanyId = dto.CompanyId
        };

        /// <summary>Edit form model. PrimaryUnitCode and SecondaryUnitCode are set by the caller.</summary>
        public static WarehouseTransModifyDto ToModifyDto(this WarehouseTransaction src) => new()
        {
            Id = src.Id,
            TransDate = src.TransDate,
            TransWarehouseDocSeriesId = src.TransWarehouseDocSeriesId,
            TransRefCode = src.TransRefCode,
            WarehouseItemId = src.WarehouseItemId,
            FiscalPeriodId = src.FiscalPeriodId,
            TransactionType = src.TransactionType,
            InventoryAction = src.InventoryAction,
            InventoryValueAction = src.InventoryValueAction,
            SectionId = src.SectionId,
            PrimaryUnitId = src.PrimaryUnitId,
            SecondaryUnitId = src.SecondaryUnitId,
            UnitFactor = src.UnitFactor,
            Quontity1 = src.Quontity1,
            Quontity2 = src.Quontity2,
            FpaRate = src.FpaRate,
            DiscountRate = src.DiscountRate,
            UnitPrice = src.UnitPrice,
            UnitExpenses = src.UnitExpenses,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            AmountDiscount = src.AmountDiscount,
            AmountExpenses = src.AmountExpenses,
            Etiology = src.Etiology,
            CompanyId = src.CompanyId,
            Timestamp = src.Timestamp
        };

        /// <summary>
        /// Copies the fields the Edit form posts onto a loaded entity. Fields the form does not post
        /// (CreatorId, SectionId, the invoiced actions, discount rate, expenses) keep their values;
        /// the old Attach+Modified save reset them to 0. Doc type, inventory actions,
        /// TransactionType and the Trans* amounts are recalculated by the caller.
        /// </summary>
        public static void ApplyTo(this WarehouseTransModifyDto dto, WarehouseTransaction entity)
        {
            entity.TransDate = dto.TransDate;
            entity.TransWarehouseDocSeriesId = dto.TransWarehouseDocSeriesId;
            entity.TransRefCode = dto.TransRefCode;
            entity.WarehouseItemId = dto.WarehouseItemId;
            entity.FiscalPeriodId = dto.FiscalPeriodId;
            entity.PrimaryUnitId = dto.PrimaryUnitId;
            entity.SecondaryUnitId = dto.SecondaryUnitId;
            entity.UnitFactor = dto.UnitFactor;
            entity.Quontity1 = dto.Quontity1;
            entity.Quontity2 = dto.Quontity2;
            entity.FpaRate = dto.FpaRate;
            entity.UnitPrice = dto.UnitPrice;
            entity.AmountFpa = dto.AmountFpa;
            entity.AmountNet = dto.AmountNet;
            entity.AmountDiscount = dto.AmountDiscount;
            entity.Etiology = dto.Etiology;
            entity.CompanyId = dto.CompanyId;
        }

        /// <summary>EF-translatable list projection.</summary>
        public static readonly Expression<Func<WarehouseTransaction, WarehouseTransListDto>> ToListDto =
            src => new WarehouseTransListDto
            {
                Id = src.Id,
                TransDate = src.TransDate,
                TransWarehouseDocSeriesId = src.TransWarehouseDocSeriesId,
                TransWarehouseDocSeriesName = src.TransWarehouseDocSeries.Name,
                TransWarehouseDocSeriesCode = src.TransWarehouseDocSeries.Code,
                TransRefCode = src.TransRefCode,
                SectionCode = src.Section.Code,
                CreatorId = src.CreatorId,
                WarehouseItemId = src.WarehouseItemId,
                WarehouseItemName = src.WarehouseItem.Name,
                TransactionType = src.TransactionType,
                InventoryAction = src.InventoryAction,
                InventoryValueAction = src.InventoryValueAction,
                InvoicedVolumeAction = src.InvoicedVolumeAction,
                InvoicedValueAction = src.InvoicedValueAction,
                Quontity1 = src.Quontity1,
                Quontity2 = src.Quontity2,
                UnitPrice = src.UnitPrice,
                UnitExpenses = src.UnitExpenses,
                AmountFpa = src.AmountFpa,
                AmountNet = src.AmountNet,
                AmountDiscount = src.AmountDiscount,
                TransQ1 = src.TransQ1,
                TransQ2 = src.TransQ2,
                TransFpaAmount = src.TransFpaAmount,
                TransNetAmount = src.TransNetAmount,
                TransDiscountAmount = src.TransDiscountAmount,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code,
                CompanyCurrencyId = src.Company.CurrencyId
            };
    }
}
