using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IProductApiClient
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> CreateAsync(Product model);
        Task<Product?> UpdateAsync(Product model);
        Task<bool> DeleteAsync(int id);

        Task<(List<Product>, int)> SearchAsync(ProductQuery query);
    }
}