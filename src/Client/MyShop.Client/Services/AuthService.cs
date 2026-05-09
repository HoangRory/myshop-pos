using LuciferCore.Extensions;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;
using System.Text;

namespace MyShop.Client.Services
{
    public class AuthService : IAuthService, IAPI
    {
        private readonly HttpClient _http;
        private const string ApiPath = "v1/api/auth";
        public string AccountId { get; private set; } = string.Empty;

        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);
        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> LoginAsync(string username, string password, bool isHashed = false)
        {
            var account = new Account(username, password, isHashed);
            var content = new StringContent(account.ToJson(), Encoding.UTF8, "application/json");

            // SỬ DỤNG FULL URL TẠI ĐÂY
            var response = await _http.PostAsync(Url("login"), content);

            if (response.IsSuccessStatusCode)
            {
                AccountId = await response.Content.ReadAsStringAsync();
                return true;
            }

            AccountId = string.Empty;
            return false;
        }

        public async Task<bool> LogoutAsync()
        {
            var response = await _http.GetAsync(Url("logout"));

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
            var content = new StringContent(account.ToJson(), Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(Url("signup"), content);
            AccountId = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }
    }
}