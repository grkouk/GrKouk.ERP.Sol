using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;

namespace GrKouk.Web.ERP.Mappings
{
    public static class WarehouseItemMappings
    {
        /// <summary>Create form pre-filled from an existing item ("copy from"). SelectedCompanies stays empty.</summary>
        public static WarehouseItemCreateDto ToCreateDto(this WarehouseItem src) => new()
        {
            Code = src.Code,
            Name = src.Name,
            ShortDescription = src.ShortDescription,
            Description = src.Description,
            Active = src.Active,
            MainMeasureUnitId = src.MainMeasureUnitId,
            SecondaryMeasureUnitId = src.SecondaryMeasureUnitId,
            SecondaryUnitToMainRate = src.SecondaryUnitToMainRate,
            BuyMeasureUnitId = src.BuyMeasureUnitId,
            BuyUnitToMainRate = src.BuyUnitToMainRate,
            FpaDefId = src.FpaDefId,
            BarCode = src.BarCode,
            ManufacturerCode = src.ManufacturerCode,
            MaterialCategoryId = src.MaterialCategoryId,
            MaterialType = src.MaterialType,
            WarehouseItemNature = src.WarehouseItemNature,
            PriceNetto = src.PriceNetto,
            PriceBrutto = src.PriceBrutto,
            CompanyId = src.CompanyId
        };

        /// <summary>New entity from the Create form. Dates and company mappings are set by the caller.</summary>
        public static WarehouseItem ToEntity(this WarehouseItemCreateDto dto) => new()
        {
            Code = dto.Code,
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Active = dto.Active,
            MainMeasureUnitId = dto.MainMeasureUnitId ?? 0,
            SecondaryMeasureUnitId = dto.SecondaryMeasureUnitId ?? 0,
            SecondaryUnitToMainRate = dto.SecondaryUnitToMainRate,
            BuyMeasureUnitId = dto.BuyMeasureUnitId ?? 0,
            BuyUnitToMainRate = dto.BuyUnitToMainRate,
            FpaDefId = dto.FpaDefId,
            BarCode = dto.BarCode,
            ManufacturerCode = dto.ManufacturerCode,
            MaterialCategoryId = dto.MaterialCategoryId,
            MaterialType = dto.MaterialType,
            WarehouseItemNature = dto.WarehouseItemNature,
            PriceNetto = dto.PriceNetto,
            PriceBrutto = dto.PriceBrutto,
            CompanyId = dto.CompanyId
        };

        /// <summary>Edit form model. SelectedCompanies is set by the caller.</summary>
        public static WarehouseItemModifyDto ToModifyDto(this WarehouseItem src) => new()
        {
            Id = src.Id,
            Code = src.Code,
            Name = src.Name,
            ShortDescription = src.ShortDescription,
            Description = src.Description,
            Active = src.Active,
            MainMeasureUnitId = src.MainMeasureUnitId,
            SecondaryMeasureUnitId = src.SecondaryMeasureUnitId,
            SecondaryUnitToMainRate = src.SecondaryUnitToMainRate,
            BuyMeasureUnitId = src.BuyMeasureUnitId,
            BuyUnitToMainRate = src.BuyUnitToMainRate,
            FpaDefId = src.FpaDefId,
            BarCode = src.BarCode,
            ManufacturerCode = src.ManufacturerCode,
            MaterialCategoryId = src.MaterialCategoryId,
            MaterialType = src.MaterialType,
            WarehouseItemNature = src.WarehouseItemNature,
            CompanyId = src.CompanyId,
            PriceNetto = src.PriceNetto,
            PriceBrutto = src.PriceBrutto,
            Timestamp = src.Timestamp
        };

        /// <summary>
        /// Copies the fields the Edit form posts onto a loaded entity. Id, Timestamp, the dates and
        /// the company mappings are not touched (the form does not post DateCreated).
        /// </summary>
        public static void ApplyTo(this WarehouseItemModifyDto dto, WarehouseItem entity)
        {
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.ShortDescription = dto.ShortDescription;
            entity.Description = dto.Description;
            entity.Active = dto.Active;
            entity.MainMeasureUnitId = dto.MainMeasureUnitId ?? 0;
            entity.SecondaryMeasureUnitId = dto.SecondaryMeasureUnitId ?? 0;
            entity.SecondaryUnitToMainRate = dto.SecondaryUnitToMainRate;
            entity.BuyMeasureUnitId = dto.BuyMeasureUnitId ?? 0;
            entity.BuyUnitToMainRate = dto.BuyUnitToMainRate;
            entity.FpaDefId = dto.FpaDefId;
            entity.BarCode = dto.BarCode;
            entity.ManufacturerCode = dto.ManufacturerCode;
            entity.MaterialCategoryId = dto.MaterialCategoryId;
            entity.MaterialType = dto.MaterialType;
            entity.WarehouseItemNature = dto.WarehouseItemNature;
            entity.CompanyId = dto.CompanyId;
            entity.PriceNetto = dto.PriceNetto;
            entity.PriceBrutto = dto.PriceBrutto;
        }

        /// <summary>EF-translatable list projection. Url is set by the caller.</summary>
        public static readonly Expression<Func<WarehouseItem, WarehouseItemListDto>> ToListDto =
            src => new WarehouseItemListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                Active = src.Active,
                MainMeasureUnitId = src.MainMeasureUnitId,
                MainMeasureUnitCode = src.MainMeasureUnit.Code,
                BuyMeasureUnitId = src.BuyMeasureUnitId,
                BuyMeasureUnitCode = src.BuyMeasureUnit.Code,
                FpaDefId = src.FpaDefId,
                FpaDefName = src.FpaDef.Name,
                BarCode = src.BarCode,
                ManufacturerCode = src.ManufacturerCode,
                MaterialCategoryId = src.MaterialCategoryId,
                // AutoMapper left this null: the navigation is spelled "MaterialCaterory".
                MaterialCategoryName = src.MaterialCaterory.Name,
                MaterialType = src.MaterialType,
                PriceNetto = src.PriceNetto,
                PriceBrutto = src.PriceBrutto,
                WarehouseItemNature = src.WarehouseItemNature,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code
            };

        /// <summary>EF-translatable list projection, one row per item and company. Url is set by the caller.</summary>
        public static readonly Expression<Func<CompanyWarehouseItemMapping, WarehouseItemBigClass>> ToBigClass =
            src => new WarehouseItemBigClass
            {
                Id = src.WarehouseItemId,
                Code = src.WarehouseItem.Code,
                Name = src.WarehouseItem.Name,
                Active = src.WarehouseItem.Active,
                FpaDefId = src.WarehouseItem.FpaDefId,
                // Kept as AutoMapper had it (the profile mapped FpaDef.Id, not Name).
                FpaDefName = src.WarehouseItem.FpaDef.Id.ToString(),
                MaterialCategoryId = src.WarehouseItem.MaterialCategoryId,
                MaterialCategoryName = src.WarehouseItem.MaterialCaterory.Name,
                MaterialType = src.WarehouseItem.MaterialType,
                WarehouseItemNature = src.WarehouseItem.WarehouseItemNature,
                CompanyId = src.CompanyId,
                CompanyCode = src.Company.Code
            };

        /// <summary>EF-translatable projection for the item autocomplete/search endpoints.</summary>
        public static readonly Expression<Func<WarehouseItem, WarehouseItemSearchListDto>> ToSearchListDto =
            src => new WarehouseItemSearchListDto
            {
                Id = src.Id,
                Code = src.Code,
                Name = src.Name,
                Active = src.Active,
                WarehouseItemNature = src.WarehouseItemNature
            };
    }
}
