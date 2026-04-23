using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Microsoft.EntityFrameworkCore;
using Server.Handler.Report.Data;

namespace Server.Handler.Report;

public class ReportService
{
    public async Task<ResponseModel> GetRevenueReport(ReportFilter? filter)
    {
        var response = Lucifer.Rent<ResponseModel>();

        if (filter == null || filter.FromDate > filter.ToDate)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();

        var query = db.Set<Models.Order>()
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .Where(o => o.Status == 1 && o.CreatedAt >= filter.FromDate && o.CreatedAt <= filter.ToDate);

        var data = await query.ToListAsync();

        // 1. Dùng Class RevenueReport thay cho kiểu ẩn danh
        var reportData = data.GroupBy(o => GetGroupKey(o.CreatedAt ?? DateTime.Now, filter.GroupType))
            .Select(g => new RevenueReport
            {
                Time = g.Key,
                Revenue = g.Sum(o => o.FinalTotal ?? 0),
                Profit = g.Sum(o => o.FinalTotal ?? 0) - g.Sum(o => o.OrderItems.Sum(i => (i.Quantity ?? 0) * (i.Product?.ImportPrice ?? 0)))
            })
            .OrderBy(r => r.Time) // Sắp xếp string yyyy-MM-dd hoặc yyyy-Www luôn đúng
            .ToList();

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, reportData.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> GetProductReport(ReportFilter? filter)
    {
        var response = Lucifer.Rent<ResponseModel>();

        if (filter == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();

        var rawData = await db.Set<Models.OrderItem>()
            .Include(i => i.Order)
            .Include(i => i.Product)
            .Where(i => i.Order.Status == 1 && i.Order.CreatedAt >= filter.FromDate && i.Order.CreatedAt <= filter.ToDate)
            .ToListAsync();

        // 2. Dùng Class ProductReport và TimeSeriesPoint
        var reportData = rawData.GroupBy(i => i.Product?.Name ?? "N/A")
            .Select(productGroup => new ProductReport
            {
                ProductName = productGroup.Key,
                Series = productGroup.GroupBy(i => GetGroupKey(i.Order.CreatedAt ?? DateTime.Now, filter.GroupType))
                    .Select(timeGroup => new TimeSeriesPoint
                    {
                        Time = timeGroup.Key,
                        Quantity = timeGroup.Sum(i => i.Quantity ?? 0)
                    })
                    .OrderBy(t => t.Time)
                    .ToList()
            })
            .ToList();

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, reportData.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    private string GetGroupKey(DateTime date, int type)
    {
        return type switch
        {
            1 => date.ToString("yyyy-MM-dd"), // 2026-04-23
            2 => $"{date.Year}-W{System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, System.Globalization.DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DayOfWeek.Monday):D2}", // 2026-W17
            3 => date.ToString("yyyy-MM"),    // 2026-04
            _ => date.ToString("yyyy")        // 2026
        };
    }
}
