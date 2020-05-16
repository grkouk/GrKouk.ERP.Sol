using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrKouk.Erp.Domain.Shared
{
    public class ProductRecipeLine
    {
        public int Id { get; set; }
        [ForeignKey("ProductRecipe")]
        public int ProductRecipeId { get; set; }
        public ProductRecipe ProductRecipe { get; set; }
        public int ProductId { get; set; }
        public WarehouseItem Product { get; set; }
        public int PrimaryUnitId { get; set; }
        public int SecondaryUnitId { get; set; }
        public Single Factor { get; set; }
        public double Quantity1 { get; set; }
        public double Quantity2 { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitExpenses { get; set; }
        public decimal AmountNet { get; set; }
        [MaxLength(500)]
        public string Etiology { get; set; }

    }
}