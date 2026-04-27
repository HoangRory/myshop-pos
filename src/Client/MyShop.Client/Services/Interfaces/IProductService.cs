using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IProductService
    {
        Task<(List<Product>, int)> GetProductsAsync(ProductQuery query);
        Task<Product?> CreateAsync(ProductEditModel model);
        Task<Product?> UpdateAsync(ProductEditModel model);
        Task<bool> DeleteAsync(int id);
    }
}
