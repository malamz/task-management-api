using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Repositories;

public class TaskRepository : Repository<Models.Task>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<(ICollection<Models.Task> Tasks, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? status = null,
        string? priority = null,
        Guid? assignedUserId = null)
    {
        var query = _dbSet
            .Include(t => t.AssignedUser)
            .AsQueryable();

        // ── Filters ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Models.TaskStatus>(status, out var parsedStatus))
            query = query.Where(t => t.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<Models.TaskPriority>(priority, out var parsedPriority))
            query = query.Where(t => t.Priority == parsedPriority);

        if (assignedUserId.HasValue)
            query = query.Where(t => t.AssignedUserId == assignedUserId.Value);

        // ── Count before paging ──────────────────────────────────────────────
        int totalCount = await query.CountAsync();

        // ── Page ─────────────────────────────────────────────────────────────
        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    /// <summary>Get a single task with its assigned-user navigation loaded.</summary>
    public new async Task<Models.Task?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
