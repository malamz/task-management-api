namespace TaskManagementAPI.Models;

// ─── Enums ───────────────────────────────────────────────────────────────────

public enum UserRole
{
    User,
    Admin
}

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
