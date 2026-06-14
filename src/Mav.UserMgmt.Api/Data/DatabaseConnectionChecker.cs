using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Mav.UserMgmt.Api.Data;

public class DatabaseConnectionChecker : IDatabaseConnectionChecker
{
    private readonly string? _connectionString;
    private readonly ILogger<DatabaseConnectionChecker> _logger;

    public DatabaseConnectionChecker(IConfiguration configuration, ILogger<DatabaseConnectionChecker> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _logger = logger;
    }

    public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            return false;
        }

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connectivity check failed.");
            return false;
        }
    }
}
