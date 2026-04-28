using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetCategoryAsync(int CategoryId);
        Task<bool> CreateAsync(Category model);
        Task<bool> UpdateAsync(Category model);
        Task<bool> DeleteAsync(int CategoryId);
    }
}
