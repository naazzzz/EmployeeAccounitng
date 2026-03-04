using General.Exceptions;
using General.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace General.Base;

public abstract class BaseRepository<T> : IRepository<T>
    where T : class
{
    protected readonly DbContext Context;

    public BaseRepository(DbContext context)
    {
        Context = context;
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await Context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await Context.Set<T>().ToListAsync();
    }

    public virtual async Task<T?> AddWithSaveAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteWithSaveAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) throw new RecordNotFoundException(id, nameof(T));

        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
    }

    public T? GetById(string id)
    {
        return Context.Set<T>().Find(id);
    }

    public async Task<int> SaveChanges()
    {
        return await Context.SaveChangesAsync();
    }

    public object GetContext()
    {
        return Context;
    }
}