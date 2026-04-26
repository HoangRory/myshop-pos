using LuciferCore.Attributes;
using LuciferCore.Main;
using LuciferCore.Utf8;

namespace Server.Database;

public static class DBConfig
{
    [Config("DBHost", "localhost")]
    public static string Host { get; set; } = string.Empty;

    [Config("DBPort", "1433")]
    public static int Port { get; set; } = default;

    [Config("DBUser", "sa")]
    public static string User { get; set; } = string.Empty;

    [Config("DBPassword", "svcntt")]
    public static string Password { get; set; } = string.Empty;

    [Config("DBName", "MyShop")]
    public static string Database { get; set; } = string.Empty;

    [Config("DBTrustServerCertificate", "true")]
    public static bool TrustServerCertificate { get; set; } = default;

    [Config("DBConnectTimeout", "15")]
    public static int ConnectTimeout { get; set; } = default;

    [Config("DBCommandTimeout", "30")]
    public static int CommandTimeout { get; set; } = default;

    [Config("DBMaxPoolSize", "100")]
    public static int MaxPoolSize { get; set; } = default;

    [Config("DBAppName", "LuciferServer")]
    public static string ApplicationName { get; set; } = "LuciferServer";

    public static string GetConnectstring()
    {
        using var builder = Lucifer.Rent<Utf8Builder>();
        builder.Append("Server=").Append(Host).Append(",").Append(Port).Append(";")
            .Append("Database=").Append(Database).Append(";")
            .Append("User=").Append(User).Append(";")
            .Append("Password=").Append(Password).Append(";")
            .Append("TrustServerCertificate=").Append(TrustServerCertificate).Append(";")
            .Append("Connect Timeout=").Append(ConnectTimeout).Append(";")
            .Append("Max Pool Size=").Append(MaxPoolSize).Append(";")
            .Append("Application Name=").Append(ApplicationName).Append(";");
        return builder.ToString();
    }
}
