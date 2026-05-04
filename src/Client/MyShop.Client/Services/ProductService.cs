using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.IO;
using System.Net.Http.Headers;
namespace MyShop.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "v1/api/product";

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Product>>(BaseUrl) ?? new();
        }

        public async Task<bool> CreateAsync(Product model)
        {
            var payload = new
            {
                model.Sku,
                model.Name,
                model.ImportPrice,
                model.SalePrice,
                model.StockCount,
                model.Description,
                model.CategoryId
            };

            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(BaseUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode && result == "Success";
        }

        public async Task<bool> UpdateAsync(Product model)
        {
            var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PutAsync($"{BaseUrl}", content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "Updated";
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var json = JsonSerializer.Serialize(new { ProductId = id });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, BaseUrl)
            {
                Content = content
            };
            var response = await _http.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode &&  result == "Deleted";
        }

        public async Task<(List<Product>, int)> SearchAsync(ProductQuery query)
        {
            var json = JsonSerializer.Serialize(query);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/search", content);

            var raw = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("Data");
            var total = doc.RootElement.GetProperty("Total").GetInt32();

            return (JsonSerializer.Deserialize<List<Product>>(data.GetRawText()) ?? new(), total);
        }
        /// <summary>
        /// Import products from Excel file (raw byte array, not multipart, not JSON)
        /// </summary>
        /// <param name="filePath">Path to Excel file</param>
        /// <returns>True if import succeeded</returns>
        public async Task<bool> ImportExcelAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

            using var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var response = await _http.PostAsync($"{BaseUrl}/import", content);
            return response.IsSuccessStatusCode;
        }
    }
}