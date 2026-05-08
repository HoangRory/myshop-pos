using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "v1/api/order";

        public OrderService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Order>>(BaseUrl) ?? new();
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            var json = $$"""
            {
                "OrderId": {{id}}
            }
            """;

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/id")
            {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json")
            };

            Console.WriteLine(json);

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Order>() ?? new();
        }
        public async Task<Order?> CreateAsync(List<OrderItem> items)
        {
            var json = JsonSerializer.Serialize(
                items,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null,
                    DefaultIgnoreCondition =
                        System.Text.Json.Serialization
                            .JsonIgnoreCondition.WhenWritingNull
                });

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(
                BaseUrl,
                content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Order>(
                result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<Order?> UpdateAsync(Order model)
        {
            var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _http.PutAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Order>(
                result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var json = JsonSerializer.Serialize(new { OrderId = id });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, BaseUrl)
            {
                Content = content
            };
            var response = await _http.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode && result == "Deleted";
        }

        public async Task<(List<Order>, int)> SearchAsync(OrderQuery query)
        {
            var json = JsonSerializer.Serialize(query);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/search", content);

            var raw = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("Data");
            var total = doc.RootElement.GetProperty("Total").GetInt32();

            return (JsonSerializer.Deserialize<List<Order>>(data.GetRawText()) ?? new(), total);
        }
    }
}
