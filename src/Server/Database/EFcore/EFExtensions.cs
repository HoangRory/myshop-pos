using Microsoft.EntityFrameworkCore;
using Server.Database.EFcore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Server.Database.EFcore;

/// <summary>
/// Các extension mở rộng cho Entity Framework Core.
/// </summary>
public static class EFExtensions
{
    /// <summary>
    /// Thực thi truy vấn SQL thô và trả về danh sách các thực thể của kiểu T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="sql"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<List<T>> FromSql<T>(this DbContext context, FormattableString sql) where T : class
        => await context.Set<T>().FromSqlInterpolated(sql).ToListAsync();

    /// <summary>
    /// Thực thi truy vấn SQL thô và trả về danh sách các thực thể của kiểu T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="sql"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<List<T>> FromSql<T>(this DbContext context, string sql) where T : class
        => await context.Set<T>().FromSqlRaw(sql).ToListAsync();

    /// <summary>
    /// Lấy tất cả bản ghi của entity T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<List<T>> GetAllAsync<T>(this DbContext context) where T : class
        => await context.Set<T>().ToListAsync();

    /// <summary>
    /// Lấy bản ghi theo khóa chính
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> GetByIdAsync<T>(this DbContext context, params object[] keyValues) where T : class
        => await context.Set<T>().FindAsync(keyValues);

    /// <summary>
    /// Lấy danh sách bản ghi theo điều kiện → dùng Expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<List<T>> WhereAsync<T>(this DbContext context, Expression<Func<T, bool>> predicate) where T : class
        => await context.Set<T>().Where(predicate).ToListAsync();

    /// <summary>
    /// Lấy bản ghi đầu tiên theo điều kiện → dùng Expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> FirstOrDefaultAsync<T>(this DbContext context, Expression<Func<T, bool>> predicate) where T : class
        => await context.Set<T>().FirstOrDefaultAsync(predicate);

    /// <summary>
    /// Lấy danh sách bản ghi phân trang
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="context"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="descending"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<List<T>> GetPagedAsync<T, TKey>(this DbContext context, int pageIndex, int pageSize, Expression<Func<T, TKey>> orderBy, bool descending = false) where T : class
    {
        var query = context.Set<T>().AsQueryable();

        query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

        return await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Đếm số bản ghi của entity T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> CountAsync<T>(this DbContext context) where T : class
        => await context.Set<T>().CountAsync();

    /// <summary>
    /// Cập nhật entity T, chỉ cập nhật các property có giá trị khác null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateNoNull<T>(this DbContext context, T entity) where T : class
    {
        var dbSet = context.Set<T>();
        var entry = context.Entry(entity);

        if (entry.State == EntityState.Detached) dbSet.Attach(entity);

        var keyNames = context.GetPrimaryKeyNames<T>();
        var props = typeof(T).GetProperties();

        foreach (var prop in props)
        {
            if (keyNames.Contains(prop.Name)) continue;

            if (prop.GetMethod?.IsVirtual == true && !prop.GetMethod.IsFinal) continue;

            if (!prop.CanWrite || !prop.CanRead) continue;

            var value = prop.GetValue(entity);
            if (value != null)
            {
                context.Entry(entity).Property(prop.Name).IsModified = true;
            }
        }
    }

    /// <summary>
    /// Thêm mới entity T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Insert<T>(this DbContext context, T entity) where T : class
        => context.Set<T>().Add(entity);

    /// <summary>
    /// Xóa entity T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Delete<T>(this DbContext context, T entity) where T : class
        => context.Set<T>().Remove(entity);

    /// <summary>
    /// Lấy tên property khóa chính (chỉ hỗ trợ khóa đơn)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetPrimaryKeyName<T>(this DbContext context) where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        var key = entityType!.FindPrimaryKey();

        // Trường hợp có composite key thì lấy property đầu tiên
        return key!.Properties[0].Name;
    }

    /// <summary>
    /// Lấy tên tất cả các property khóa chính (hỗ trợ composite key)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<string> GetPrimaryKeyNames<T>(this DbContext context) where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        var key = entityType!.FindPrimaryKey();

        return key!.Properties.Select(p => p.Name).ToList();
    }
}
