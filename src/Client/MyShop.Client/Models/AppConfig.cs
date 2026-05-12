using LuciferCore.Extensions;
using System.IO;

namespace MyShop.Client.Models
{
    public class AppConfig
    {
        public string LastViewModel { get; set; } = "Dashboard";
        public string StartupScreen { get; set; } = "Settings";
        public string ServerIP { get; set; } = "localhost";
        public string ServerPort { get; set; } = "8443";
        public int ItemsPerPage { get; set; } = 10;
        public bool RememberLastScreen { get; set; } = false;

        public string GetServerUrl() => $"https://{ServerIP}:{ServerPort}/";

        private const string FileName = "config.json";

        public void Save()
        {
            string json = this.ToJson();
            File.WriteAllText(FileName, json);
        }

        public static AppConfig Load()
        {
            if (!File.Exists(FileName)) return new AppConfig();
            try
            {
                string json = File.ReadAllText(FileName);
                return System.Text.Json.JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch { return new AppConfig(); }
        }
    }
}
