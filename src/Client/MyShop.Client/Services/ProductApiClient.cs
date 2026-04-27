using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.Services
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "v1/api/product";

        public ProductApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Product>>(BaseUrl) ?? new();
        }

        public async Task<Product?> CreateAsync(Product model)
        {
            var response = await _http.PostAsJsonAsync(BaseUrl, model);
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task<Product?> UpdateAsync(Product model)
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/{model.Id}", model);
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<(List<Product>, int)> SearchAsync(ProductQuery query)
        {
            var json = JsonSerializer.Serialize(query, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/search", content);

            var raw = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("Data");
            var total = doc.RootElement.GetProperty("Total").GetInt32();

            return (JsonSerializer.Deserialize<List<Product>>(data.GetRawText()) ?? new(), total);
        }
    }
}