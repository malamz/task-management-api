
using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Contracts;

// ─── Auth DTOs ───────────────────────────────────────────────────────────────

/// <summary>Payload sent to POST /api/auth/register.</summary>
public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = "";

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = "";

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
}

/// <summary>Payload sent to POST /api/auth/login.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

/// <summary>Response returned after a successful login.</summary>
public class AuthResponse
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}


// ─── Shared ──────────────────────────────────────────────────────────────────

/// <summary>Generic API envelope returned on errors.</summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public IEnumerable<string>? Errors { get; set; }
}
