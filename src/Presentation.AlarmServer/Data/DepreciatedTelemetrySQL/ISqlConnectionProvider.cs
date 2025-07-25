using Microsoft.Data.SqlClient;

namespace Presentation.AlarmServer.Data.TelemetrySQL;

public interface ISqlConnectionProvider
{
    SqlConnection GetOpenConnection();
}
