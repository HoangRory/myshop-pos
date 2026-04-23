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

        // Thiết lập mốc thời gian
        var now = DateTime.Now;
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var data = new Dashboard();

        try
        {
            // 1. Chỉ số cơ bản (Dùng AsNoTracking cho tất cả các lệnh Read-only)
            data.TotalProducts = await db.Set<Product>().AsNoTracking().CountAsync();

            // Lấy danh sách FinalTotal của các đơn hàng Status = 1 (Thành công) trong ngày hôm nay
            var ordersToday = await db.Set<Order>()
                .AsNoTracking()
                .Where(o => o.CreatedAt >= today && o.Status == 1)
                .Select(o => o.FinalTotal)
                .ToListAsync();

            data.TotalOrdersToday = ordersToday.Count;
            data.TotalRevenueToday = ordersToday.Sum(x => x ?? 0);

            // 2. Top 5 sản phẩm sắp hết hàng (Stock < 5)
            data.LowStockProducts = await db.Set<Product>()
                .AsNoTracking()
                .Where(p => p.StockCount < 5)
                .OrderBy(p => p.StockCount)
                .Take(5)
                .ToListAsync();

            // 3. 3 đơn hàng gần nhất (Hiển thị feed hoạt động mới nhất)
            data.RecentOrders = await db.Set<Order>()
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ToListAsync();

            // 4. Top 5 sản phẩm bán chạy nhất tháng (Dựa trên số lượng đã bán)
            data.BestSellingProducts = await db.Set<OrderItem>()
                .AsNoTracking()
                .Where(oi => oi.Order.Status == 1 && oi.Order.CreatedAt >= firstDayOfMonth)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                // Join ngược lại bảng Product để lấy đầy đủ thông tin hiển thị (Name, Image...)
                .Join(db.Set<Product>().AsNoTracking(),
                    x => x.ProductId,
                    p => p.ProductId,
                    (x, p) => p)
                .ToListAsync();

            // 5. Monthly Revenue Chart (Dữ liệu doanh thu từng ngày từ đầu tháng đến nay)
            var monthlyOrders = await db.Set<Order>()
                .AsNoTracking()
                .Where(o => o.CreatedAt >= firstDayOfMonth && o.Status == 1)
                .Select(o => new { Day = o.CreatedAt!.Value.Day, o.FinalTotal })
                .ToListAsync();

            // Gom nhóm theo ngày vào Dictionary để truy xuất O(1)
            var revenueByDay = monthlyOrders
                .GroupBy(o => o.Day)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalTotal ?? 0));

            // Đổ dữ liệu vào list theo thứ tự từ ngày 1 đến ngày hôm nay
            for (int day = 1; day <= today.Day; day++)
            {
                data.MonthlyRevenueChart.Add(revenueByDay.GetValueOrDefault(day, 0));
            }

            response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, data.ToJson(), StorageData.ApplicationJson);
        }
        catch (Exception ex)
        {
            // Trả về lỗi chi tiết nếu có vấn đề truy vấn
            response.MakeCustomResponse<byte, char, byte>(500, StorageData.Http11Protocol, ex.Message, StorageData.TextPlainCharset);
        }

        return response;
    }
}
