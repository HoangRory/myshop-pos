using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Server.Database.EFcore.Sql;

/// <summary>
/// DbContext chính của ứng dụng.
/// </summary>
public class EFSqlContext : DbContext
{
    private static IEnumerable<Type>? _cachedEntityTypes;
    public EFSqlContext()
    {

    }

    public EFSqlContext(DbContextOptions<EFSqlContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Chỉ cấu hình nếu bên ngoài chưa cấu hình (linh hoạt tối đa)
        if (!optionsBuilder.IsConfigured)
        {
            try
            {
                optionsBuilder.UseSqlServer(DBConfig.GetConnectstring(), sqlOptions =>
                {
                    // Thiết lập Command Timeout mặc định cho tất cả truy vấn qua DbContext này
                    sqlOptions.CommandTimeout(DBConfig.CommandTimeout); // 30 giây

                    // Có thể cấu hình thêm các tính năng như tự động kết nối lại khi rớt mạng
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
                Console.WriteLine($"[Database] Đã cấu hình kết nối DB thành công: {DBConfig.Host}:{DBConfig.Port}/{DBConfig.Database} pass: {DBConfig.Password}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database] Lỗi khi cấu hình kết nối DB: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Cấu hình mô hình dữ liệu khi tạo DbContext.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Quét toàn bộ AppDomain để tìm class có [Table]
        _cachedEntityTypes ??= AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => SafeGetTypes(a))
        .Where(t => t.GetCustomAttribute<TableAttribute>() != null && !t.IsAbstract);

        foreach (var type in _cachedEntityTypes)
        {
            modelBuilder.Entity(type);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EFSqlContext).Assembly);
    }
    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }
}