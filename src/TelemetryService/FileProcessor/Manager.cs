using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading;
using NLog;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class Manager: BackgroundService
    {
        private const int FILE_POLL_INTERVAL = 3000; // milliseconds

        private static readonly Logger nLogger = LogManager.GetCurrentClassLogger();

        private static readonly string connectionString;

        static Manager()
        {
            // We could initialise the connection string when it was first used, but here is simpler as there's no need to deal with threading.
            connectionString = ConfigurationManager.AppSettings["TelemetryDatabaseConnectionString"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            FilePoller poller = new FilePoller();
            while (!stoppingToken.IsCancellationRequested)
            {
                poller.Poll();
                await Task.Delay(FILE_POLL_INTERVAL, stoppingToken);
            }
        }

        /// <summary>
        /// Obtain an open and valid connection to the telemetry database.
        /// </summary>
        /// <remarks>Must be thread-safe.</remarks>
        public static SqlConnection GetSqlConnection()
        {
            try
            {
                SqlConnection cn = new SqlConnection(connectionString);
                cn.Open();
                return cn;
            }
            catch (Exception ex)
            {
                nLogger.Error(ex, "Couldn't acquire SQL connection");
                return null;
            }
        }
    }
}
