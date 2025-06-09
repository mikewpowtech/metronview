using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelemetryServer.Options;

namespace TelemetryServer.Infrastructure;

public class SqlHelper
{
    private ILogger<SqlHelper> logger { get; }
    private string connectionString { get; }

    public SqlHelper(IOptions<SqlHelperOptions> options, ILogger<SqlHelper> logger)
    {
        connectionString = options.Value.ConnectionString;
        this.logger = logger;
    }

    /// <summary>
    /// Obtain an open and valid connection to the telemetry database.
    /// </summary>
    /// <remarks>Must be thread-safe.</remarks>
    public SqlConnection GetSqlConnection()
    {
        try
        {
            SqlConnection cn = new(connectionString);
            cn.Open();
            return cn;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Couldn't acquire SQL connection");
            return null;
        }
    }
}