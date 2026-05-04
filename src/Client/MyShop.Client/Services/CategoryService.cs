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
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "v1/api/category";

        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Category>>(BaseUrl) ?? new();
        }
        public async Task<Category?> GetCategoryAsync(int categoryId)
        {
            var json = JsonSerializer.Serialize(categoryId);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync($"{BaseUrl}/id", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Category>(
                result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<bool> CreateAsync(Category model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(BaseUrl, content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }

        public async Task<bool> UpdateAsync(Category model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(BaseUrl, content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }
    }
}
