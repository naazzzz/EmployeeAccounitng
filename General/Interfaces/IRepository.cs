namespace General.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    T? GetById(string id);
    Task<T?> AddWithSaveAsync(T entity);
    Task DeleteWithSaveAsync(string id);
    Task<int> SaveChanges();

    object GetContext();
}