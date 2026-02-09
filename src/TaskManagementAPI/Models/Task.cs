namespace TaskManagementAPI.Models;

// ─── Task Entity ─────────────────────────────────────────────────────────────

/// <summary>
/// Represents a task stored in the Tasks table.
/// </summary>
public class Task
{
    /// <summary>Primary key (auto-generated GUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Short title of the task (max 200 chars).</summary>
    public string Title { get; set; } = "";

    /// <summary>Optional long-form description.</summary>
    public string? Description { get; set; }

    /// <summary>Current workflow status.</summary>
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    /// <summary>Urgency level.</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>Optional target completion date.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>FK – the user this task is assigned to (nullable).</summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC last-update timestamp.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ───────────────────────────────────────────────────────────
    /// <summary>The user this task is assigned to.</summary>
    public User? AssignedUser { get; set; }
}
