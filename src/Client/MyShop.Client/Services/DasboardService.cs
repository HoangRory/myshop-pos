using LuciferCore.Attributes;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace MyShop.Client.Services
{
    [Plugin("Service", "Dashboard")]
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private const string ApiPath = "/v1/api/dashboard";
        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);
        public DashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dashboard> GetDashboardDataAsync()
            => await _httpClient.GetFromJsonAsync<Dashboard>(Url()) ?? new();
    }
}
