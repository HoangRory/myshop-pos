using MyShop.Client.Models.Report;
using Server.Handler.Report;

namespace MyShop.Client.Services.Interfaces
{
    public interface IReportService : IAPI
    {
        Task<List<ProductReport>> GetProductReportAsync(ReportFilter filter);
        Task<List<RevenueReport>> GetRevenueReportAsync(ReportFilter filter);
    }
}
