using TaskManagementAPI.Contracts;
using TaskManagementAPI.Models;
using Serilog;

namespace TaskManagementAPI.Interfaces;

/// <summary>Task CRUD business-logic.</summary>
public interface ITaskService
{
    Task<PaginatedResponse<TaskResponse>> GetTasksAsync(int page, int pageSize, string? status, string? priority, Guid? assignedUserId);
    Task<TaskResponse> GetTaskByIdAsync(Guid id);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    System.Threading.Tasks.Task DeleteTaskAsync(Guid id);
}
