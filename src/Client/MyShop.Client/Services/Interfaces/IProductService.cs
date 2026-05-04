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
        /// <summary>
        /// Import products from Excel file (raw byte array, not multipart, not JSON)
        /// </summary>
        /// <param name="filePath">Path to Excel file</param>
        /// <returns>True if import succeeded</returns>
        Task<bool> ImportExcelAsync(string filePath);
    }
}