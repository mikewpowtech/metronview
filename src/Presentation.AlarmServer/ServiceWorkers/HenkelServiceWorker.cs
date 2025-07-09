using Presentation.AlarmServer.Alarms;
using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Email;
using Presentation.AlarmServer.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Models;
using Presentation.AlarmServer.Data.SpiderScope;
using Presentation.AlarmServer.Helpers;
using Presentation.AlarmServer.Enums;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;

namespace Presentation.AlarmServer.ServiceWorkers;

//using primary constructor
public class HenkelServiceWorker(ITelemetryDatabase telemetryDatabase, ILoggerFactory loggerFactory,
    IOptions<WorkerOptions> workerOptions, ISpiderScopeApi spiderScopeApi, IAlarmServerService alarmService) 
    : ServiceWorkerBase(telemetryDatabase, loggerFactory, workerOptions, alarmService)
{
    /// <summary>
    /// Poll the spiderscope API for Henkel RTU status and configuration changes.
    /// Transition from OK to OK : do nothing.
    /// Transition from OK to BreachedUpper queue 1234, ALARM, ON,
    /// Transition from BreachedUpper to OK queue 1234,ALARM,OFF,
    /// Transition from OK to BreachedLower queue 1234, ALARM, ON,
    /// Transition from BreachedLower to OK queue 1234,ALARM,OFF,    
    /// </summary>
    /// 
    public override async void Run(CancellationToken cancellationToken)
    {
        base.Run(cancellationToken);
        if (workerOptions.HenkelPollInterval == 0) { logger.LogTrace("HenkelPollInterval=0, Exit Run()"); }
        else
        {
            logger.LogTrace($"Enter HenkelPoller Run interval {workerOptions.HenkelPollInterval}");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var response = await spiderScopeApi.GetAsync<List<RtuResponse>>("monitor/alarms");
                    //is the unit defined in metronview? - use a custom field, id 'c117-spc-usestatus' if set to 1 then it is defined in metronview
                    //do the comparison of the incoming status with the current status
                    //queue a configuration if necessary
                    //update the status
                    //NB response.id is the manufacturerId
                    logger.LogInformation($"Spiderscope returned: {response.Count} items");
                    if (response?.Count > 0)
                    {
                        var enabledRtus = telemetryDatabase.GetEnabledCustomFieldValues(workerOptions);
                        logger.LogInformation($"GetEnabledCustomFieldValues returned: {enabledRtus.Count} items");
                        if (enabledRtus.Count > 0)
                        {
                            //if we have got this far, process the rtus
                            ProcessHenkelRtus(enabledRtus, response);
                        }
                    }
                    PauseThisWorker(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception bubbled up to HenkelPoller.Run()");
                    // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                    cancellationToken.WaitHandle.WaitOne(Math.Max(workerOptions.ErrorPauseInterval, workerOptions.HenkelPollInterval));
                }

            logger.LogTrace("Exit Henkelpoller Run()");
        }

        void PauseThisWorker(CancellationToken cancellationToken)
        {
            logger.LogInformation($"HenkelPoller waiting: {workerOptions.HenkelPollInterval} milliseconds");
            cancellationToken.WaitHandle.WaitOne(workerOptions.HenkelPollInterval);
        }
    }

    private void ProcessHenkelRtus(IList<HenkelRtuStatus> enabledRtus, List<RtuResponse> apiRtuList)
    {
        foreach (var enabledRtu in enabledRtus)
        {
            var rtuResponse = apiRtuList.FirstOrDefault(r => r.Id == enabledRtu.ManufacturerId);
            if (rtuResponse == null) continue;

            if (!Enum.TryParse<HenkelStatusType>(enabledRtu.Status??"Unknown", true, out var enabledRtuStatus))
            {
                logger.LogWarning($"Invalid status '{enabledRtu.Status}' for RTU {enabledRtu.RtuId}");
                continue;
            }

            //conditional expression
            var configuration = enabledRtuStatus switch
            {
                HenkelStatusType.Ok when rtuResponse.Status == HenkelStatusType.BreachedUpper => $"1234,ALARM,ON,",
                HenkelStatusType.Ok when rtuResponse.Status == HenkelStatusType.BreachedLower => $"1234,ALARM,ON,",
                HenkelStatusType.BreachedUpper when rtuResponse.Status == HenkelStatusType.Ok => $"1234,ALARM,OFF,",
                HenkelStatusType.BreachedLower when rtuResponse.Status == HenkelStatusType.Ok => $"1234,ALARM,OFF,",
                HenkelStatusType.Unknown when rtuResponse.Status == HenkelStatusType.Ok => $"1234,ALARM,OFF,",
                HenkelStatusType.Unknown when rtuResponse.Status == HenkelStatusType.BreachedUpper => $"1234,ALARM,ON,",
                HenkelStatusType.Unknown when rtuResponse.Status == HenkelStatusType.BreachedLower => $"1234,ALARM,ON,",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(configuration))
            {
                //queue a configuration
                logger.LogInformation($"Processing, setting configuration - {configuration}");
                telemetryDatabase.AddConfigrationUpload(enabledRtu, configuration);
                //update the status
                telemetryDatabase.SetHenkelStatusCustomFieldValueCache(enabledRtu, rtuResponse.Status, workerOptions);
            }

        }
    }

}
