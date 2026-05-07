using System.Collections.Generic;
using System.Threading.Tasks;
using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(long id);
        Task<bool> CreateAsync(Order model);
        Task<bool> UpdateAsync(Order model);
        Task<bool> DeleteAsync(long id);

        Task<(List<Order>, int)> SearchAsync(OrderQuery query);
    }
}
