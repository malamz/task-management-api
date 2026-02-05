
using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Contracts;

// ─── Task DTOs ───────────────────────────────────────────────────────────────

/// <summary>Payload sent to POST /api/tasks.</summary>
public class CreateTaskRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string? Description { get; set; }

    public string Status { get; set; } = "Todo";       // validated in service
    public string Priority { get; set; } = "Medium";   // validated in service

    public DateTime? DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
}

/// <summary>Payload sent to PUT /api/tasks/{id}.</summary>
public class UpdateTaskRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
}

/// <summary>Response object for a single task – hides internal navigation properties.</summary>
public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public DateTime? DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Paginated list wrapper.</summary>
public class PaginatedResponse<T>
{
    public ICollection<T> Data { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
