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
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;

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
            .WriteTo.Async(a => a.File(
                new Serilog.Formatting.Json.JsonFormatter(),                
                "logs/app-.json",
                rollingInterval: RollingInterval.Day,
                buffered: true, // Buffers writes for speed
                flushToDiskInterval: TimeSpan.FromSeconds(1))); // Flushes every second            
            
    });

    // ─── Configuration ───────────────────────────────────────────────────────    
    builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();

    // ─── Services ────────────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();    
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ITaskService, TaskService>();

    // ─── Rate Limiting ───────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(rateLimiterOptions =>
    {
        rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        rateLimiterOptions.AddFixedWindowLimiter("AuthPolicy", options =>
        {
            options.Window = TimeSpan.FromMinutes(1);
            options.PermitLimit = 10;
        });
        rateLimiterOptions.AddFixedWindowLimiter("GlobalPolicy", options =>
        {
            options.Window = TimeSpan.FromMinutes(1);
            options.PermitLimit = 60;
        });
    });

    // ─── CORS ────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // ─── Swagger ─────────────────────────────────────────────────────────────
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Task Management API",
            Version = "v1",
            Description = "A production-ready REST API for task management with JWT authentication and role-based authorization."
        });

        // Swagger JWT bearer token support
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter your JWT token in the field below.\r\n\r\nExample: \"eyJhbGci...\"",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            { new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
        });

        // Include XML documentation
        var xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
        if (File.Exists(xmlFile))
            options.IncludeXmlComments(xmlFile);
    });

    builder.Services.AddControllers();

    var app = builder.Build();

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseGlobalExceptionHandler();       // Custom error handler
    app.UseHttpsRedirection();
    app.UseCors("AllowSpecificOrigins");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // ─── Swagger (dev only by default – remove env check to always expose) ──
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
            options.RoutePrefix = "swagger";
        });
    }

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
