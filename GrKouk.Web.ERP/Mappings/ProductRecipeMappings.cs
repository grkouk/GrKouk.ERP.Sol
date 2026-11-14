using System;
using System.Linq.Expressions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.ProductRecipies;

namespace GrKouk.Web.ERP.Mappings
{
    public static class ProductRecipeMappings
    {
        public static readonly Expression<Func<ProductRecipe, ProductRecipeDto>> ToDto = src => new ProductRecipeDto
        {
            Id = src.Id,
            ProductName = src.Product.Name,
            CompanyName = src.Company.Name
        };
    }
}
