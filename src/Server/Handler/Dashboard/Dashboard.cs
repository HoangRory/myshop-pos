namespace Server.Handler.Dashboard;

public class Dashboard
{
    public int? TotalProducts { get; set; }
    public int? TotalOrdersToday { get; set; }
    public decimal? TotalRevenueToday { get; set; }

    // Top 5 sản phẩm sắp hết hàng
    public List<Models.Product?> LowStockProducts { get; set; } = new();

    // Top 5 sản phẩm bán chạy
    public List<Models.Product?> BestSellingProducts { get; set; } = new();

    // 3 đơn hàng gần nhất
    public List<Models.Order?> RecentOrders { get; set; } = new();

    // Doanh thu theo tháng hiện tại từ ngày 1 đến ngày hôm nay
    public List<decimal?> MonthlyRevenueChart { get; set; } = new();
}
