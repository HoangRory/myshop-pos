using LuciferCore.Extensions;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MyShop.Client.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public Account() { }

        public Account(string username, string password, bool isHashed = false)
        {
            Username = username;
            PasswordHash = isHashed ? password : HashPassword(password);
        }

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // --- Logic Lưu File JSON ---
        public void SaveToFile(string filePath)
        {
            try
            {
                var json = this.ToJson();
                File.WriteAllText(filePath, json);
            }
            catch (Exception)
            {
                // Xử lý log lỗi nếu cần
            }
        }

        // --- Logic Load File JSON (Trả về Account hoặc null) ---
        public static Account? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                // Dùng FileStream để cho phép các tiến trình khác vẫn có thể truy cập file
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                string jsonString = reader.ReadToEnd();

                return JsonSerializer.Deserialize<Account>(jsonString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JSON Error]: {ex.Message}");
                return null;
            }
        }

    }
}
