using System.Collections.Generic;
using System.Linq;
using MyShop.Client.Models;
using MyShop.Shared.DTOs;
using MyShop.Shared.Requests;

namespace MyShop.Client.Mappers
{
    public static class ProductMapper
    {
        public static Product ToModel(ProductDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                SKU = dto.SKU,
                SalePrice = dto.SalePrice,
                ImportPrice = dto.ImportPrice,
                StockCount = dto.StockCount,
                Description = dto.Description,
                CategoryId = dto.CategoryId
            };
        }

        public static List<Product> ToModelList(List<ProductDto> dtos)
        {
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            return dtos.Select(ToModel).ToList();
        }

        public static CreateProductRequest ToCreateRequest(Product model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new CreateProductRequest
            {
                Name = model.Name,
                SKU = model.SKU,
                SalePrice = model.SalePrice,
                ImportPrice = model.ImportPrice,
                StockCount = model.StockCount,
                Description = model.Description,
                CategoryId = model.CategoryId
            };
        }

        public static UpdateProductRequest ToUpdateRequest(Product model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new UpdateProductRequest
            {
                Id = model.Id,
                Name = model.Name,
                SKU = model.SKU,
                SalePrice = model.SalePrice,
                ImportPrice = model.ImportPrice,
                StockCount = model.StockCount,
                Description = model.Description,
                CategoryId = model.CategoryId
            };
        }
    }
}
