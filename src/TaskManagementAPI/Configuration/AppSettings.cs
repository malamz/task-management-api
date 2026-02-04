namespace TaskManagementAPI.Configuration;

/// <summary>
/// Database configuration values bound from appsettings.json → "Database".
/// </summary>
public class DatabaseSettings
{
    /// <summary>When true the app uses Npgsql; otherwise SQL Server.</summary>
    public bool UsePostgres { get; set; }

    /// <summary>ADO.NET connection string.</summary>
    public string ConnectionString { get; set; } = "";
}
