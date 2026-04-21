using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services
{
    public class ProductApiClient
    {
        public async Task<List<Product>> GetProductsAsync()
        {
            // Simulate network delay
            await Task.Delay(500);

            // Mock API response returning 3 sample products
            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop Pro", Price = 1299.99m },
                new Product { Id = 2, Name = "Wireless Mouse", Price = 29.99m },
                new Product { Id = 3, Name = "Mechanical Keyboard", Price = 89.99m }
            };
        }
    }
}
