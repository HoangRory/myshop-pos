using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Shared.DTOs;
using MyShop.Shared.Requests;
using MyShop.Shared.Responses;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.Services
{
    public class ProductApiClient : IProductApiClient
    {
        public async Task<ApiResponse<List<ProductDto>>> GetAllAsync()
        {
            await Task.Delay(500);

            return new ApiResponse<List<ProductDto>>
            {
                Data = new List<ProductDto>
                {
                    new ProductDto { Id = 1, Name = "Laptop Pro", SalePrice = 1299.99m },
                    new ProductDto { Id = 2, Name = "Wireless Mouse", SalePrice = 29.99m },
                    new ProductDto { Id = 3, Name = "Mechanical Keyboard", SalePrice = 89.99m }
                },
                Success = true
            };
        }

        public Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<ApiResponse<ProductDto>> UpdateAsync(UpdateProductRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}