using System;
using System.Collections.Generic;

namespace GrKouk.Erp.Domain.Shared
{
    public class ProductRecipe
    {
        private ICollection<ProductRecipeLine> _productRecipeLines;
        public int Id { get; set; }
        public int ProductId { get; set; }
        public WarehouseItem Product { get; set; }
        public int PrimaryUnitId { get; set; }
        public int SecondaryUnitId { get; set; }
        public Single Factor { get; set; }
        public double Quantity1 { get; set; }
        public double Quantity2 { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public ICollection<ProductRecipeLine> ProductRecipeLines
        {
            get { return _productRecipeLines ?? (_productRecipeLines = new List<ProductRecipeLine>()); }
            set { _productRecipeLines = value; }
        }

    }
}
