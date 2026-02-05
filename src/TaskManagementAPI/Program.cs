using TaskManagementAPI.Data;
using TaskManagementAPI.Configuration;
using Serilog;
using TaskManagementAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Repositories;
using TaskManagementAPI.Services;

// ─── Serilog Bootstrap Logger (before DI is built) ──────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog (full) ──────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day);
    });

    // ─── Configuration ───────────────────────────────────────────────────────
    
    builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

    // ─── Database ────────────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var dbSettings = builder.Configuration.GetSection("Database").Get<DatabaseSettings>()!;
        if (dbSettings.UsePostgres)
            options.UseNpgsql(dbSettings.ConnectionString);
        else
            options.UseSqlServer(dbSettings.ConnectionString);
    });

    // ─── Authentication & Authorization ─────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

    builder.Services.AddAuthorization();

    // ─── Repositories ────────────────────────────────────────────────────────
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    

    // ─── Services ────────────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();    
    builder.Services.AddScoped<ITokenService, TokenService>();


    builder.Services.AddControllers();

    var app = builder.Build();

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseGlobalExceptionHandler();       // Custom error handler
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();


    app.MapControllers();

    // ─── Auto-Migrate on startup ─────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbSeeder.SeedAsync(dbContext); // seeds a default Admin user
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
    return;
}
finally
{
    Log.CloseAndFlush();
}
