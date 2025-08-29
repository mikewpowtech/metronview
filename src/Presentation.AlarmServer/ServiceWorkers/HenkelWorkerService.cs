using Application.ConfigurationUploads;
using Application.CustomFields;
using Application.CustomFields.Dtos;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Data.SpiderScope;
using Presentation.AlarmServer.Models;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

public class HenkelWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HenkelWorkerService> _logger;
    private readonly WorkerOptions _workerOptions;
    private readonly string _className;
    private readonly ISpiderScopeApi _spiderScopeApi;

    public HenkelWorkerService(IServiceProvider serviceProvider, ILogger<HenkelWorkerService> logger,
        IOptions<WorkerOptions> workerOptions, ISpiderScopeApi spiderScopeApi)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _workerOptions = workerOptions.Value;
        _spiderScopeApi = spiderScopeApi;
        _className = GetType().Name;
    }

    /// <summary>
    /// Poll the spiderscope API for Henkel RTU status and configuration changes.
    /// Transition from OK to OK : do nothing.
    /// Transition from OK to BreachedUpper queue 1234, ALARM, ON,
    /// Transition from BreachedUpper to OK queue 1234,ALARM,OFF,
    /// Transition from OK to BreachedLower queue 1234, ALARM, ON,
    /// Transition from BreachedLower to OK queue 1234,ALARM,OFF,    
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Enter HenkelPoller ExecuteAsync interval {_workerOptions.HenkelPollInterval}");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_workerOptions.HenkelPollInterval == 0)
                {
                    _logger.LogInformation("HenkelPollInterval=0, Exit ExecuteAsync()");
                    await Task.Delay(_workerOptions.DefaultPauseInterval, stoppingToken);
                }
                else
                {
                    var response = await _spiderScopeApi.GetAsync<List<RtuResponse>>("monitor/alarms");
                    //is the unit defined in metronview? - use a custom field, id 'c117-spc-usestatus' if set to 1 then it is defined in metronview
                    //do the comparison of the incoming status with the current status
                    //queue a configuration if necessary
                    //update the status
                    //NB response.id is the manufacturerId
                    _logger.LogInformation($"Spiderscope returned: {response.Count} items");

                    if (response?.Count > 0)
                    {
                        // Create a scope for each iteration to get fresh service instances
                        using var scope = _serviceProvider.CreateScope();
                        var customFieldService = scope.ServiceProvider.GetRequiredService<ICustomFieldService>();
                        var configurationUploadService = scope.ServiceProvider.GetRequiredService<IConfigurationUploadService>();

                        var enabledRtus = customFieldService.GetEnabledCustomFieldValues
                            (_workerOptions.HenkelUseSpiderScopeStatusCustomFieldName, _workerOptions.HenkelSpiderScopeStatusCustomFieldName);
                        _logger.LogInformation($"GetEnabledCustomFieldValues returned: {enabledRtus.Count} items");

                        if (enabledRtus.Count > 0)
                        {
                            //if we have got this far, process the rtus
                            ProcessHenkelRtus(enabledRtus, response, customFieldService, configurationUploadService);
                        }
                    }

                    await Task.Delay(_workerOptions.HenkelPollInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception bubbled up to HenkelPoller.ExecuteAsync()");
                // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                await Task.Delay(Math.Max(_workerOptions.ErrorPauseInterval, _workerOptions.HenkelPollInterval), stoppingToken);
            }
        }

        _logger.LogTrace("Exit HenkelPoller ExecuteAsync()");
    }

    private void ProcessHenkelRtus(IList<HenkelRtuStatus> enabledRtus, List<RtuResponse> apiRtuList, 
        ICustomFieldService customFieldService, IConfigurationUploadService configurationUploadService)
    {
        foreach (var enabledRtu in enabledRtus)
        {
            var rtuResponse = apiRtuList.FirstOrDefault(r => r.Id == enabledRtu.ManufacturerId);
            if (rtuResponse == null) continue;

            if (!Enum.TryParse<HenkelStatusType>(enabledRtu.Status ?? "Unknown", true, out var enabledRtuStatus))
            {
                _logger.LogWarning($"Invalid status '{enabledRtu.Status}' for RTU {enabledRtu.RtuId}");
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
                _logger.LogInformation($"Processing, setting configuration - {configuration}");
                configurationUploadService.AddConfigrationUpload(enabledRtu, configuration);
                //update the status
                customFieldService.SetHenkelStatusCustomFieldValueCache(enabledRtu, rtuResponse.Status, _workerOptions.HenkelSpiderScopeStatusCustomFieldName);
            }
        }
    }
}
