using TaskManagementAPI.Contracts;
using TaskManagementAPI.Models;
using Serilog;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;
    private readonly IUserRepository _userRepo;

    public TaskService(ITaskRepository taskRepo, IUserRepository userRepo)
    {
        _taskRepo = taskRepo;
        _userRepo = userRepo;
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    public async Task<PaginatedResponse<TaskResponse>> GetTasksAsync(
        int page, int pageSize, string? status, string? priority, Guid? assignedUserId)
    {
        var (tasks, total) = await _taskRepo.GetPaginatedAsync(page, pageSize, status, priority, assignedUserId);

        return new PaginatedResponse<TaskResponse>
        {
            Data = tasks.Select(MapToResponse).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskResponse> GetTaskByIdAsync(Guid id)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Task with id '{id}' was not found.");

        return MapToResponse(task);
    }

    // ── Write ────────────────────────────────────────────────────────────────

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request)
    {
        // Validate enum values
        if (!Enum.TryParse<Models.TaskStatus>(request.Status, out var status))
            throw new ArgumentException($"Invalid status '{request.Status}'. Allowed: Todo, InProgress, Done.");

        if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
            throw new ArgumentException($"Invalid priority '{request.Priority}'. Allowed: Low, Medium, High.");

        // Validate assigned user exists (if supplied)
        if (request.AssignedUserId.HasValue)
        {
            var user = await _userRepo.GetByIdAsync(request.AssignedUserId.Value);
            if (user is null)
                throw new KeyNotFoundException($"User with id '{request.AssignedUserId}' was not found.");
        }

        var task = new Models.Task
        {
            Title = request.Title,
            Description = request.Description,
            Status = status,
            Priority = priority,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId
        };

        task = await _taskRepo.AddAsync(task);
        Log.Information("Task created: {TaskId} – {Title}", task.Id, task.Title);

        // Re-fetch so the navigation property is populated
        return MapToResponse(await _taskRepo.GetByIdAsync(task.Id) ?? task);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await _taskRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Task with id '{id}' was not found.");

        // Apply only non-null fields (partial update)
        if (request.Title != null)
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.Status != null)
        {
            if (!Enum.TryParse<Models.TaskStatus>(request.Status, out var status))
                throw new ArgumentException($"Invalid status '{request.Status}'.");
            task.Status = status;
        }

        if (request.Priority != null)
        {
            if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
                throw new ArgumentException($"Invalid priority '{request.Priority}'.");
            task.Priority = priority;
        }

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        if (request.AssignedUserId.HasValue)
        {
            var user = await _userRepo.GetByIdAsync(request.AssignedUserId.Value);
            if (user is null)
                throw new KeyNotFoundException($"User with id '{request.AssignedUserId}' was not found.");
            task.AssignedUserId = request.AssignedUserId;
        }

        task.UpdatedAt = DateTime.UtcNow;
        task = await _taskRepo.UpdateAsync(task);

        Log.Information("Task updated: {TaskId}", task.Id);
        return MapToResponse(await _taskRepo.GetByIdAsync(task.Id) ?? task);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(Guid id)
    {
        var deleted = await _taskRepo.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Task with id '{id}' was not found.");

        Log.Information("Task deleted: {TaskId}", id);
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static TaskResponse MapToResponse(Models.Task task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status.ToString(),
        Priority = task.Priority.ToString(),
        DueDate = task.DueDate,
        AssignedUserId = task.AssignedUserId,
        AssignedUserName = task.AssignedUser?.Name,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
