using System;

namespace GrKouk.Web.ERP.Helpers;

public class CashierItemCreateDto
{
    
    public int Id { get; set; }
      
    public string Code { get; set; }
       
    public string Name { get; set; }
    
    public bool Active { get; set; }
    
    public int MainMeasureUnitId { get; set; }
    
    public int SecondaryMeasureUnitId { get; set; }
    
    public double SecondaryUnitToMainRate { get; set; }
    
    public int BuyMeasureUnitId { get; set; }
    
    public double BuyUnitToMainRate { get; set; }
      
    public int FpaDefId { get; set; }
      
    public string BarCode { get; set; }
      
    public string ManufacturerCode { get; set; }

    public int MaterialCategoryId { get; set; }
       
    public int MaterialType { get; set; }
    public int WarehouseItemNature { get; set; }
    public decimal PriceNetto { get; set; }
    public decimal PriceBrutto { get; set; }
      

    public DateTime ModifiedAt { get; set; }
       
}