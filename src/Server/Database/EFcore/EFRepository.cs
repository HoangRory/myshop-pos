using LuciferCore.Main;
using Microsoft.EntityFrameworkCore;
using Server.Contract;

namespace Server.Database.EFcore;

public class EFRepository<T> : IRepository<T> where T : class
{
    public async Task<T?> GetByIdAsync(object id)
    {
        using var context = Lucifer.GetModelT<DbContext>();
        return await context.GetByIdAsync<T>(id);
    }

    public async Task<IEnumerable<T>> GetAsync(object? filter = null)
    {
        using var context = Lucifer.GetModelT<DbContext>();

        if (filter is System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return await context.WhereAsync(predicate);
        }

        return await context.GetAllAsync<T>();
    }

    public async Task<int> AddAsync(T entity)
    {
        using var context = Lucifer.GetModelT<DbContext>();
        context.Insert(entity);
        return await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        using var context = Lucifer.GetModelT<DbContext>();
        context.UpdateNoNull(entity);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(object id)
    {
        using var context = Lucifer.GetModelT<DbContext>();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity == null) return 0;

        context.Delete(entity);
        return await context.SaveChangesAsync();
    }
}
