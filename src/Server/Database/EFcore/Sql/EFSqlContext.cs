using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Server.Database.EFcore.Sql;

/// <summary>
/// DbContext chính của ứng dụng.
/// </summary>
public class EFSqlContext : DbContext
{
    private readonly string _connectionString;
    private static IEnumerable<Type>? _cachedEntityTypes;
    public EFSqlContext()
    {
        _connectionString = "Data Source=localhost;Initial Catalog=MyShop;User ID=sa;Password=svcntt;TrustServerCertificate=True;";
    }
    public EFSqlContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    public EFSqlContext(DbContextOptions<EFSqlContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Chỉ cấu hình nếu bên ngoài chưa cấu hình (linh hoạt tối đa)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
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