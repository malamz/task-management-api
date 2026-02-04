using TaskManagementAPI.Data;
using TaskManagementAPI.Configuration;
using Serilog;
using TaskManagementAPI.Middleware;
using Microsoft.EntityFrameworkCore;

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


    builder.Services.AddControllers();

    var app = builder.Build();

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseGlobalExceptionHandler();       // Custom error handler
    app.UseHttpsRedirection();
        

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
