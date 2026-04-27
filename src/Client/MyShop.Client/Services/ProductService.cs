using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductApiClient _apiClient;
        public ProductService(IProductApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<(List<Product>, int)> GetProductsAsync(ProductQuery query)
        {
            return await _apiClient.SearchAsync(query);
        }

        public async Task<Product?> CreateAsync(ProductEditModel model)
        {
            var product = new Product
            {
                Name = model.Name,
                SKU = model.SKU,
                SalePrice = model.SalePrice,
                StockCount = model.StockCount,
                Id = model.Id
            };
            var result = await _apiClient.CreateAsync(product);
            return result;
        }

        public async Task<Product?> UpdateAsync(ProductEditModel model)
        {
            var product = new Product
            {
                Id = model.Id,
                Name = model.Name,
                SKU = model.SKU,
                SalePrice = model.SalePrice,
                StockCount = model.StockCount
            };
            var result = await _apiClient.UpdateAsync(product);
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _apiClient.DeleteAsync(id);
        }
    }
}
