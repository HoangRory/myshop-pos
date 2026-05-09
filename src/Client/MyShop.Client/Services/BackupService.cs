using LuciferCore.Extensions;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyShop.Client.Services
{
    public class BackupService : IBRService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "/v1/api/backup-restore";
        public BackupService(HttpClient http)
        {
            _http = http;
        }
        public async Task<bool> CreateBackupAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/backup");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBackupAsync(string bkName)
        {
            var json = JsonSerializer.Serialize(new { Name = bkName });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, BaseUrl)
            {
                Content = content
            };

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<BackupRestore>> GetAllBackupsAsync()
            => await _http.GetFromJsonAsync<List<BackupRestore>>(BaseUrl) ?? [];

        public async Task<bool> RestoreAsync(string bkName)
        {
            var payload = new { Name = bkName }.ToJson();
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/restore", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetAutoBackupAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/auto-backup");
            return response.IsSuccessStatusCode;
        }
    }
}
