
namespace TaskManagementAPI.Interfaces;

// ─── Interface ───────────────────────────────────────────────────────────────

/// <summary>
/// Generic repository that every entity repo inherits.
/// Keeps the service layer database-agnostic.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
