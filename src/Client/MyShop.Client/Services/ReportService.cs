using LuciferCore.Attributes;
using LuciferCore.Extensions;
using MyShop.Client.Models.Report;
using MyShop.Client.Services.Interfaces;
using Server.Handler.Report;
using System.Net.Http;

namespace MyShop.Client.Services
{
    [Plugin("Service", "Report")]
    public class ReportService : IReportService
    {
        private readonly HttpClient _http;
        private const string ApiPath = "v1/api/report";

        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);
        public ReportService(HttpClient http)
        {
            _http = http;
        }

        public Task<List<ProductReport>> GetProductReportAsync(ReportFilter filter)
        => PostReportAsync<List<ProductReport>>("product", filter);

        public Task<List<RevenueReport>> GetRevenueReportAsync(ReportFilter filter)
            => PostReportAsync<List<RevenueReport>>("revenue", filter);
        private async Task<T> PostReportAsync<T>(string endpoint, ReportFilter filter) where T : new()
        {
            var content = new StringContent(filter.ToJson(), System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Url(endpoint), content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return json.FromJson<T>() ?? new T();
            }

            return new T();
        }
    }
}
