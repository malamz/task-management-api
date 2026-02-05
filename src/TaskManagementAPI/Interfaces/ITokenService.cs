
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Interfaces;

/// <summary>Generates signed JWT tokens.</summary>
public interface ITokenService
{
    /// <summary>Build a signed JWT for the supplied user.</summary>
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
