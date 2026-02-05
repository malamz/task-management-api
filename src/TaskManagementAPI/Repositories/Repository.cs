using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Repositories;

// ─── Base Repository Implementation ─────────────────────────────────────────────────────

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Added;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null) return false;

        _context.Entry(entity).State = EntityState.Deleted;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
