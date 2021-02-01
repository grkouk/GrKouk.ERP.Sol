using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GrKouk.Web.ERP.Helpers
{
    public class ProductInfoResponse
    {
        public int FpaId { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PriceNetto { get; set; }
        public decimal PriceBrutto { get; set; }
        public string WarehouseItemName { get; set; }
        public float FpaRate { get; set; }
        public int MainUnitId { get; set; }
        public string MainUnitCode { get; set; }
        public int SecondaryUnitId { get; set; }
        public string SecondaryUnitCode { get; set; }
        public double SecondaryFactor { get; set; }
        public IList<ProductUnit> ProductUnits { get; set; }

    }

    public class ProductUnit
    {
        public int UnitId { get; set; }
        public UnitTypeEnum UnitType { get; set; }
        public string UnitName { get; set; }
        public string UnitCode { get; set; }
        public double UnitFactor { get; set; }
        public int TransactorId { get; set; }
        public bool IsDefault { get; set; }
    }

    public enum UnitTypeEnum
    {
        [Description("Base Unit")]
        BaseUnitType = 1,
        [Description("Secondary Unit")]
        SecondaryUnitType = 2,
        [Description("Buy Unit")]
        BuyUnitType = 3,
        [Description("Supplier Buy Unit")]
        SupplierBuyUnitType = 4
    }
}
