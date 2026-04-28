using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<bool> CreateAsync(Product model);
        Task<bool> UpdateAsync(Product model);
        Task<bool> DeleteAsync(int id);

        Task<(List<Product>, int)> SearchAsync(ProductQuery query);
    }
}