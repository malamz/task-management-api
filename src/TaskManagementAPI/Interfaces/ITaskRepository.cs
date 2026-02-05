
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Interfaces;

/// <summary>Task-specific query methods on top of the generic <see cref="IRepository{TEntity}"/>.</summary>
public interface ITaskRepository : IRepository<Models.Task>
{
    /// <summary>
    /// Returns a paginated, optionally filtered page of tasks.
    /// </summary>
    Task<(ICollection<Models.Task> Tasks, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? status = null,
        string? priority = null,
        Guid? assignedUserId = null);
}
