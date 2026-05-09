using LuciferCore.Attributes;
using LuciferCore.Extensions;
using System.Text;


namespace Server.Core;

public static class AppLicense
{
    [Config("LicenseKey", "FREE_TRIAL")]
    private static string _licenseKey { get; set; } = string.Empty;

    // Giả sử Key xịn là một chuỗi Hash nào đó liên quan đến tên máy/CPU
    private static string ValidKey = "LUCIFER-PRO-2026";

    public static bool IsValid { get; private set; } = false;
    public static bool IsTrialExpired()
    {
        // Nếu đã nhập đúng Key thì không cần check ngày Trial nữa
        if (_licenseKey == ValidKey) return false;

        var installDate = LicenseShield.GetInstallDate();
        var trialDays = (DateTime.Now - installDate).TotalDays;

        return trialDays > 15;
    }

    [ConsoleCommand("/check license")]
    public static void CheckLicense()
    {
        if (_licenseKey == ValidKey)
        {
            typeof(AppLicense).LogConsole("License: [Activated] Professional Version.");
            IsValid = true;
        }
        else if (IsTrialExpired())
        {
            typeof(AppLicense).LogConsole("License: [Expired] Trial period ended. Please enter License Key.");
            IsValid = false;
        }
        else
        {
            var daysLeft = Math.Max(15 - (DateTime.Now - LicenseShield.GetInstallDate()).TotalDays, 0);
            typeof(AppLicense).LogConsole($"License: [Trial] {Math.Round(daysLeft, 1)} days remaining. Please enter License Key to continue using after trial expires.");
            IsValid = daysLeft > 0;
        }
    }
}

public static class LicenseShield
{
    // Đặt tên file trông như file hệ thống để "ngụy trang"
    private static readonly string LicenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".sys_init_lock");

    public static DateTime GetInstallDate()
    {
        try
        {
            if (!File.Exists(LicenseFilePath))
            {
                var now = DateTime.Now;
                // Lưu ticks của ngày hiện tại, XOR nhẹ để tránh nhìn thô
                string secureData = Convert.ToBase64String(Encoding.UTF8.GetBytes((now.Ticks ^ 123456).ToString()));
                File.WriteAllText(LicenseFilePath, secureData);
                return now;
            }

            // Đọc và giải mã
            string content = File.ReadAllText(LicenseFilePath);
            byte[] data = Convert.FromBase64String(content);
            long ticks = long.Parse(Encoding.UTF8.GetString(data)) ^ 123456;

            return new DateTime(ticks);
        }
        catch
        {
            // Nếu file bị hỏng hoặc lỗi, mặc định cho chạy tiếp (hoặc khóa tùy bạn)
            return DateTime.Now;
        }
    }
}
