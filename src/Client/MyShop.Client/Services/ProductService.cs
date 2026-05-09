using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
namespace MyShop.Client.Services
{
    public class ProductService : IProductService, IAPI
    {
        private readonly HttpClient _http;
        private const string ApiPath = "v1/api/product";

        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Product>>(Url()) ?? new();
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

            var response = await _http.PostAsync(Url(), content);
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

            var response = await _http.PutAsync(Url(), content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "Updated";
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var json = JsonSerializer.Serialize(new { ProductId = id });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, Url())
            {
                Content = content
            };
            var response = await _http.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "Deleted";
        }

        public async Task<(List<Product>, int)> SearchAsync(ProductQuery query)
        {
            var json = JsonSerializer.Serialize(query);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(Url("search"), content);

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

            var response = await _http.PostAsync(Url("import"), content);
            return response.IsSuccessStatusCode;
        }
    }
}