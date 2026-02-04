
namespace TaskManagementAPI.Data;

/// <summary>
/// Seeds an initial Admin user so the database is usable immediately after
/// the first migration.  Runs inside the startup scope after MigrateAsync.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed when no users exist yet
        if (context.Users.Any())
            return;

        var adminEmail = "admin@example.com";
        var adminPassword = "Admin123!"; // change before first real deployment

        var admin = new Models.User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Name = "Admin",
            Role = Models.UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
