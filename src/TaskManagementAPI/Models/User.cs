namespace TaskManagementAPI.Models;

// ─── User Entity ─────────────────────────────────────────────────────────────

/// <summary>
/// Represents an application user stored in the Users table.
/// </summary>
public class User
{
    /// <summary>Primary key (auto-generated GUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique email address used for login.</summary>
    public string Email { get; set; } = "";

    /// <summary>BCrypt password hash – never stored in plain text.</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>User's display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Role that drives authorization policy checks.</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>UTC timestamp of account creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ───────────────────────────────────────────────────────────
    /// <summary>Tasks assigned to this user (one-to-many).</summary>
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
}
