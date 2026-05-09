using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyShop.Client.Services
{
    public class CategoryService : ICategoryService, IAPI
    {
        private readonly HttpClient _http;
        private const string ApiPath = "v1/api/category";
        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);
        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Category>>(Url()) ?? new();
        }
        public async Task<Category?> GetCategoryAsync(int categoryId)
        {
            var json = JsonSerializer.Serialize(categoryId);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync(Url("id"), content);

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
            var response = await _http.PostAsync(Url(), content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }

        public async Task<bool> UpdateAsync(Category model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(Url(), content);
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync(Url("id"));
            var result = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode && result == "OK";
        }
    }
}
