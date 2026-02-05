
using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Contracts;

// ─── Shared ──────────────────────────────────────────────────────────────────

/// <summary>Generic API envelope returned on errors.</summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public IEnumerable<string>? Errors { get; set; }
}
