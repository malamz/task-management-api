namespace TaskManagementAPI.Configuration;

/// <summary>
/// JWT configuration values bound from appsettings.json → "Jwt".
/// </summary>
public class JwtSettings
{
    /// <summary>Symmetric signing key – must be ≥ 32 characters in production.</summary>
    public string SecretKey { get; set; } = "";

    /// <summary>Token issuer claim value.</summary>
    public string Issuer { get; set; } = "TaskManagementAPI";

    /// <summary>Token audience claim value.</summary>
    public string Audience { get; set; } = "TaskManagementAPIClients";

    /// <summary>Token lifetime in minutes.</summary>
    public int ExpirationMinutes { get; set; } = 60;
}

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
