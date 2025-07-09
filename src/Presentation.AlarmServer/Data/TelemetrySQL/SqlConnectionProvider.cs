using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.Data.TelemetrySQL;

public class SqlConnectionProvider : ISqlConnectionProvider
{
    private readonly ILogger<SqlConnectionProvider> logger;
    private readonly WorkerOptions workerOptions;

    public SqlConnectionProvider(IOptions<WorkerOptions> workerOptions, ILogger<SqlConnectionProvider> logger)
    {
        this.logger = logger;
        this.workerOptions = workerOptions.Value;
    }

    /// <summary>
    ///     Obtain an open and valid connection to the telemetry database.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Must be thread-safe and re-entrant.</remarks>
    public SqlConnection GetOpenConnection()
    {
        try
        {
            var cn = new SqlConnection(workerOptions.TelemetryDatabaseConnectionString);
            cn.Open();
            return cn;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Couldn't acquire SQL connection {connstring}", workerOptions.TelemetryDatabaseConnectionString);
            return null;
        }
    }
}