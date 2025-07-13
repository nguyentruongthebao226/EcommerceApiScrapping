using System.Linq.Expressions;

namespace EcommerceApiScrapingService.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
        Task<T> CreateAsync(T entity);
        Task<bool> ReplaceAsync(string id, T entity);
        Task<bool> DeleteAsync(string id);
    }
}
