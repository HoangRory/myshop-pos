using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IDashboardService : IAPI
    {
        Task<Dashboard> GetDashboardDataAsync();
    }
}
