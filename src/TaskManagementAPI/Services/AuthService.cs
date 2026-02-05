using TaskManagementAPI.Contracts;
using TaskManagementAPI.Models;
using Serilog;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Duplicate check
        var existing = await _userRepository.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException($"An account with email '{request.Email}' already exists.");

        // 2. Build user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Role = UserRole.User          // new sign-ups are always plain users
        };

        user = await _userRepository.AddAsync(user);

        Log.Information("New user registered: {Email} (Role={Role})", user.Email, user.Role);

        // 3. Return token immediately – no separate login step required
        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        Log.Information("User logged in: {Email}", user.Email);

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
    }
}
