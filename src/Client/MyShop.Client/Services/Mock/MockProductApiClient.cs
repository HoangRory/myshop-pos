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
                //tạo dữ liệu mẫu cho tôi
                new ProductDto { Id = 1, Name = "Laptop Pro", SalePrice = 1299.99m, Description = "A high-end laptop for professionals." },
                new ProductDto { Id = 2, Name = "Smartphone X", SalePrice = 999.99m, Description = "A flagship smartphone with cutting-edge features." },
                new ProductDto { Id = 3, Name = "Wireless Earbuds", SalePrice = 199.99m, Description = "Premium wireless earbuds with noise cancellation." },
                new ProductDto { Id = 4, Name = "Gaming Console", SalePrice = 499.99m, Description = "Next-gen gaming console with stunning graphics." },
                new ProductDto { Id = 5, Name = "4K Monitor", SalePrice = 399.99m, Description = "Ultra HD monitor for immersive gaming and productivity." },
                new ProductDto { Id = 6, Name = "Mechanical Keyboard", SalePrice = 89.99m, Description = "A durable mechanical keyboard with customizable RGB lighting." },
                new ProductDto { Id = 7, Name = "Wireless Mouse", SalePrice = 29.99m, Description = "Ergonomic wireless mouse with long battery life." },
                new ProductDto { Id = 8, Name = "External SSD", SalePrice = 149.99m, Description = "Portable external SSD with fast read/write speeds." },
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
