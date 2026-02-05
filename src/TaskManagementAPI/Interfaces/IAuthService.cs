using TaskManagementAPI.Contracts;
using TaskManagementAPI.Repositories;
using TaskManagementAPI.Models;
using Serilog;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Interfaces;

/// <summary>Orchestrates user registration and login.</summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
