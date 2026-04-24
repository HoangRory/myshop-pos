using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyShop.Client.Services.Interfaces;
using MyShop.Shared.DTOs;
using MyShop.Shared.Requests;
using MyShop.Shared.Responses;

namespace MyShop.Client.Services.Mock
{
    public class MockProductApiClient : IProductApiClient
    {
        private readonly List<ProductDto> _products;
        private int _nextId;

        public MockProductApiClient()
        {
            _products = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product A", SalePrice = 10000, Description = "Sample product A" },
                new ProductDto { Id = 2, Name = "Product B", SalePrice = 20000, Description = "Sample product B" },
                new ProductDto { Id = 3, Name = "Product C", SalePrice = 30000, Description = "Sample product C" }
            };
            _nextId = _products.Max(x => x.Id) + 1;
        }

        public async Task<ApiResponse<List<ProductDto>>> GetAllAsync()
        {
            await Task.Delay(300);
            return new ApiResponse<List<ProductDto>>
            {
                Data = _products.ToList(),
                Success = true
            };
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request)
        {
            await Task.Delay(300);
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return new ApiResponse<ProductDto> { Success = false, Message = "Invalid product data." };
            }
            var product = new ProductDto
            {
                Id = _nextId++,
                Name = request.Name,
                SalePrice = request.SalePrice,
                Description = request.Description
            };
            _products.Add(product);
            return new ApiResponse<ProductDto> { Data = product, Success = true };
        }

        public async Task<ApiResponse<ProductDto>> UpdateAsync(UpdateProductRequest request)
        {
            await Task.Delay(300);
            var product = _products.FirstOrDefault(x => x.Id == request.Id);
            if (product == null)
            {
                return new ApiResponse<ProductDto> { Success = false, Message = "Product not found." };
            }
            product.Name = request.Name;
            product.SalePrice = request.SalePrice;
            product.Description = request.Description;
            return new ApiResponse<ProductDto> { Data = product, Success = true };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            await Task.Delay(300);
            var product = _products.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return new ApiResponse<bool> { Data = false, Success = false, Message = "Product not found." };
            }
            _products.Remove(product);
            return new ApiResponse<bool> { Data = true, Success = true };
        }
    }
}
