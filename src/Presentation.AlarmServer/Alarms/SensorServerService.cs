using Application.Triggers;
using Microsoft.Extensions.Logging;
using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Models;
using Substituter;
using System;
using System.Linq;
using System.Text;

namespace Presentation.AlarmServer.Alarms;

//use a primary constructor for consistency and brevity
public class SensorServerService(ITelemetryDatabase telemetryDatabase, ILogger<SensorServerService> logger) : ISensorServerService
{
    private static readonly string[] CalculateSensor_KEYS = { "channel", "lowvalue", "highValue", "engineeringunits", "sensorname" };
    private static readonly string[] CalculateRtu_KEYS = { "rtuid", "phonenumber", "manufacturerid", "clientid" };

    /// <summary>
    ///     Substitute any {...} values in s for their values, and return the substituted string.
    /// </summary>
    /// <param name="templateSnippet"></param>
    /// <returns></returns>
    public string Substitute(BreachedTriggerDto trigger, string templateSnippet)
    {
        if (string.IsNullOrWhiteSpace(templateSnippet))
            return templateSnippet;
        var outputBuilder = new StringBuilder();

        foreach (var chunk in SubstitutionParser.ToChunkList(templateSnippet).Chunks)
        {
            if (!chunk.IsSubstitution)
            {
                outputBuilder.Append(chunk.Text);
                continue;
            }

            var value = CalculateTemplatePlaceholderValue(trigger, chunk.Text.Replace(" ", ""));
            if (null != value)
                outputBuilder.Append(value);
        }

        return outputBuilder.ToString();
    }

    public object? CalculateTemplatePlaceholderValue(BreachedTriggerDto alarm, string fieldName)
    {
        if (!alarm.KnownValues.ContainsKey(fieldName))
            CalculateAlarmKnownValue(alarm, fieldName);
        return alarm.KnownValues[fieldName];
    }
    internal void CalculateAlarmKnownValue(BreachedTriggerDto alarm, string key)
    {
        logger.LogDebug("Calculate value for template variable {Key}", key);
        // First try to get a calculator that does this by its name
        if (CalculateSensor_KEYS.Contains(key.ToLower()))
        {
            logger.LogDebug("Calculate value for template variable {Key} using CalculateSensor", key);
         //   telemetryDatabase.CalculateSensor(alarm);
            return;
        }
        if (CalculateRtu_KEYS.Contains(key.ToLower()))
        {
            logger.LogDebug("Calculate value for template variable {Key} using CalculateRtu", key);
           // telemetryDatabase.CalculateRtu(alarm);
            return;
        }

        // Failing that, try the user-defined values
        if (alarm.KnownValuesSet)
        {
            logger.LogTrace("Already loaded values, set value to null");
            // If the user values are already set and nothing else has picked it up, it's unknown
            alarm.SetKnownValue(key, null);
            return;
        }

       // CalculateCustomDataForSensor(alarm);

        alarm.KnownValuesSet = true;

        // If we still don't know it after picking up the user variables, it's unknown
        if (!alarm.KnowsValue(key))
            alarm.SetKnownValue(key, null);
    }

    private void CalculateCustomDataForSensor(AlarmServerDto alarm)
    {
        if (!telemetryDatabase.CalculateV2CustomerDataForSensor(alarm))
        {
            telemetryDatabase.CalculateV1CustomDataForSensor(alarm);
        }
    }

}
