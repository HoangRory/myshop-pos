using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Shared.DTOs;
using MyShop.Shared.Requests;
using MyShop.Shared.Responses;

namespace MyShop.Client.Services.Interfaces
{
    public interface IProductApiClient
    {
        Task<ApiResponse<List<ProductDto>>> GetAllAsync();
        Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request);
        Task<ApiResponse<ProductDto>> UpdateAsync(UpdateProductRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
