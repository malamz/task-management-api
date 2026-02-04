using Serilog;
using TaskManagementAPI.Middleware;

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

    
    builder.Services.AddControllers();

    var app = builder.Build();

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseGlobalExceptionHandler();       // Custom error handler
    app.UseHttpsRedirection();
        

    app.MapControllers();


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
