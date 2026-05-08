using LuciferCore.Extensions;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;

namespace MyShop.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "/v1/api/auth";
        public string AccountId { get; private set; } = string.Empty;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var account = new Account(username, password);
            var content = new StringContent(account.ToJson(), System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{BaseUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                AccountId = await response.Content.ReadAsStringAsync();
                return true;
            }

            AccountId = string.Empty; // Reset nếu fail
            return false;
        }

        public async Task<bool> LogoutAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/logout");

            if (response.IsSuccessStatusCode)
            {
                AccountId = string.Empty;
                return true;
            }

            return false;
        }

        public async Task<bool> SignUpAsync(string username, string password)
        {
            var account = new Account(username, password);
            var content = new StringContent(account.ToJson(), System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{BaseUrl}/signup", content);
            AccountId = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }
    }
}
