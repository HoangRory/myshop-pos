using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Server.Handler.Dashboard;

using LuciferCore.Extensions;
using Server.Models;

public class DashboardService
{
    public async Task<ResponseModel> GetDashboardData()
    {
        var response = Lucifer.Rent<ResponseModel>();
        using var db = Lucifer.GetModelT<DbContext>();
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var data = new Dashboard();

        // 1. Chỉ số cơ bản
        data.TotalProducts = await db.Set<Product>().CountAsync();

        var ordersToday = await db.Set<Order>()
            .Where(o => o.CreatedAt >= today)
            .Select(o => o.FinalTotal)
            .ToListAsync();

        data.TotalOrdersToday = ordersToday.Count;
        data.TotalRevenueToday = ordersToday.Sum();

        // 2. Top 5 sản phẩm sắp hết hàng (Stock < 5)
        data.LowStockProducts = await db.Set<Product>()
            .Where(p => p.StockCount < 5)
            .OrderBy(p => p.StockCount)
            .Take(5)
            .ToListAsync();

        // 3. 3 đơn hàng gần nhất
        data.RecentOrders = await db.Set<Order>()
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .ToListAsync();

        // 4. Top 5 sản phẩm bán chạy (Join OrderItem và Product)
        data.BestSellingProducts = await db.Set<OrderItem>()
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .Join(db.Set<Product>(), x => x.ProductId, p => p.ProductId, (x, p) => p)
            .ToListAsync();

        // 5. MonthlyRevenueChart (List<int> từ ngày 1 đến nay)
        var monthlyOrders = await db.Set<Order>()
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .Select(o => new { o.CreatedAt, o.FinalTotal })
            .ToListAsync();

        for (int day = 1; day <= today.Day; day++)
        {
            var dailySum = monthlyOrders
                .Where(o => o.CreatedAt!.Value.Day == day)
                .Sum(o => o.FinalTotal);
            data.MonthlyRevenueChart.Add(dailySum);
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, data.ToJson(), StorageData.ApplicationJson);
        return response;
    }
}
