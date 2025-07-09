using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.AlarmServer.Options;

public class WorkerOptions
{
    public int HenkelPollInterval { get; set; } = 0;
    public int AlarmPollerInterval { get; set; } = 5000;
    public int NotReportedPollInterval { get; set; } = 6000;
    public string HenkelUseSpiderScopeStatusCustomFieldName { get; set; } = "usespiderscopestatus";
    public string HenkelSpiderScopeStatusCustomFieldName { get; set; } = "spiderscope-status";
    /// <summary>
    ///     Time in milliseconds to pause after a top-level error; this is designed to quench the worst happenings if for
    ///     example the database server is down.
    /// </summary>
    public int ErrorPauseInterval { get; set; } = 10000;
    [Required]
    public string TelemetryDatabaseConnectionString { get; set; }
    public int MaximumDaysToLookBack { get; set; } = 7;
    public TimeSpan DailyRunTime { get; set; } = new TimeSpan(3, 0, 0);
    public bool RunImmediately { get; set; } = false;
    public string ReportName { get; set; }
    public string ftpServerUrl { get; set; }
    public string ftpUsername { get; set; }
    public string ftpPassword { get; set; }
    public string reportPrefix { get; set; } = "daily-";
    public string reportTimeStampFormat { get; set; } = "yyyyMMddhhmm";
}
