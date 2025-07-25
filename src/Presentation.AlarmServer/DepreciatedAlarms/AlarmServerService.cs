using Application.Alarms.Dtos;
using Application.Options;
using Application.Triggers;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Email;
using Substituter;
using System.Net.Mail;

namespace Presentation.AlarmServer.Alarms;

public class AlarmServerService(ITelemetryDatabase telemetryDatabase, ILogger<AlarmServerService> logger, 
    ISensorServerService sensorService, IMessengerService messengerService, IOptions<SmtpOptions> smtpOptions) : IAlarmServerService
{
    //private readonly Lazy<ISensorTemplateValues> customValues;
    private readonly SmtpOptions smtpOptions = smtpOptions.Value;

    /// <summary>
    ///     This is only a possible alarm - for speed in the telemetry server, the code adding readings does the bare minimum
    ///     to detect that at least one alarm is defined for the sensor.  Check whether we actually have to send the alarm.
    /// </summary>
    /// <param name="cn"></param>
    /// <returns>true if the alarm is real (and should therefore be sent), false otherwise.</returns>
    public AlarmSendingResult ShouldSendAlarmUnlessQuenched(BreachedTriggerDto alarm)
    {
        bool? result = null;

        switch (alarm.TriggerType.Code)
        {
            case TriggerTypeCode.Above:
                logger.LogDebug("Is Reading {Value} above {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
                result = !double.IsNaN(alarm.Value) && !double.IsNaN(alarm.TriggerValue) && alarm.Value >= alarm.TriggerValue;
                break;
            case TriggerTypeCode.Below:
                logger.LogDebug("Is Reading {Value} below {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
                result = !double.IsNaN(alarm.Value) && !double.IsNaN(alarm.TriggerValue) && alarm.Value <= alarm.TriggerValue;
                break;
            case TriggerTypeCode.Falling:
                result = IsFalling(alarm);
                break;
            case TriggerTypeCode.Rising:
                result = IsRising(alarm);
                break;
            case TriggerTypeCode.RateOfChange:
                logger.LogDebug("Rate of change! {IsAlarm}", alarm.IsAlarm);
                result = alarm.IsAlarm;
                break;
            case TriggerTypeCode.NotReportedForPeriod:
                logger.LogDebug("Not reported for {AlarmPeriod} minutes? {SendAlarmForNotReported}", alarm.TriggerValue, alarm.SendAlarmForNotReported);
                result = alarm.SendAlarmForNotReported;
                break;
        }

        if (result.HasValue)
        {
            logger.LogDebug("AlarmType {AlarmType}, result: {Result}", alarm.TriggerType.Code, result);
            return result.Value;
        }

        logger.LogWarning("Cannot Process Alarm#{AlarmId}: {AlarmType} is an unknown alarm type", alarm.TriggerId, alarm.TriggerType.Code);
        return new AlarmSendingResult(SendAlarmAction.Skip);
    }

    private bool IsFalling(BreachedTriggerDto alarm)
    {
        logger.LogDebug("Is Reading {Value} falling below {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
        if (double.IsNaN(alarm.Value) || double.IsNaN(alarm.TriggerValue) || alarm.Value > alarm.TriggerValue)
            return false;

        var r = telemetryDatabase.GetMostRecentReadingBefore(alarm.SensorId, alarm.DateRecordedUtc);
        logger.LogDebug("Previous reading was {@PreviousReading}", r);

        // If there is no previous reading, we assume the value is falling and trigger the alarm.
        if (r == null) return true;

        if (r.Value <= alarm.TriggerValue) return false;

        return true;
    }

    private bool IsRising(BreachedTriggerDto alarm)
    {
        logger.LogDebug("Is Reading {Value} rising past {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
        if (double.IsNaN(alarm.Value) || double.IsNaN(alarm.TriggerValue) || alarm.Value < alarm.TriggerValue)
            return false;

        var r = telemetryDatabase.GetMostRecentReadingBefore(alarm.SensorId, alarm.DateRecordedUtc);
        logger.LogDebug("Previous reading was {@PreviousReading}", r);
        if (r == null) return true;
        if (r.Value >= alarm.TriggerValue) return false;
        return true;
    }

    public void SendAlarm(BreachedTriggerDto triggerDto)
    {
        switch (triggerDto.CommunicationMode.ToAlarmRecipientType())
        {
            case AlarmRecipientType.SMS:
                SendSms(triggerDto);
                break;
            case AlarmRecipientType.Email:
                SendEmail(triggerDto);
                break;
            case AlarmRecipientType.WebService:
                SendWebService();
                break;
            case AlarmRecipientType.Unknown:
            default:
                break;
        }
    }
    internal void NoteMessageSent(BreachedTriggerDto triggerDto)
    {
        switch (triggerDto.CommunicationMode.ToAlarmRecipientType())
        {
            case AlarmRecipientType.SMS:
                NoteSmsSendCompleted();
                break;
            case AlarmRecipientType.Email:
                NoteSmtpSendCompleted();
                break;
            case AlarmRecipientType.WebService:
                NoteWebServiceSendCompleted();
                break;
            case AlarmRecipientType.Unknown:
            default:
                break;
        }
    }

    private void SendSms(BreachedTriggerDto triggerDto)
    {
        logger.LogDebug("Sending alarm via SMS");
        // There are two sets of substitutions going on here:
        // 1) Substitute the RTU/reading/... values into the alarm's body to produce {message} for the per-company SMS templates;
        var substitutedAlarmBody = sensorService.Substitute(triggerDto, triggerDto.Body);
        var substitutes = new Dictionary<string, string> { { "message", substitutedAlarmBody } };

        // 2) Substitute {sms} and {message} into the per-company SMS templates and send the resulting message.
        // Each recipient may be in a different company with their own set of templates, so handle each one as a single message.
        var toAddresses = telemetryDatabase.GetSmsToAddresses(triggerDto);
        foreach (var toAddress in toAddresses)
        {
            // Check all of the templates are populated
            if (null == toAddress.Sms)
            {
                logger.LogWarning("Cannot send SMS: company {CompanyId} {RecipientId} has null SMS field",
                    toAddress.CompanyId, toAddress.RecipientId);
                continue;
            }

            substitutes["sms"] = toAddress.Sms;
            var msg = new MailMessage
            {
                From = new MailAddress(toAddress.FromAddress),
                Subject = SubstitutionParser.Substitute(substitutes, toAddress.SubjectTemplate),
                Body = SubstitutionParser.Substitute(substitutes, toAddress.BodyTemplate),
                IsBodyHtml = false
            };
            msg.To.Add(SubstitutionParser.Substitute(substitutes, toAddress.ToAddressTemplate));
            messengerService.SendSmsAsync(msg, triggerDto);
        }
    }
    private void SendEmail(BreachedTriggerDto triggerDto)
    {
        logger.LogDebug("Sending alarm via Email");
        var substitutedSubject = sensorService.Substitute(triggerDto, triggerDto.Subject);
        var substitutedBody = sensorService.Substitute(triggerDto, triggerDto.Body);
        var isBodyHtml = substitutedBody.ToLower().Contains("<html"); // TODO: Improve this rather unpleasant hack
        var toAddresses = telemetryDatabase.GetEmailToAddresses(triggerDto);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Sending email to {@Recipients}", toAddresses.Select(_ => _.To));
        }

        foreach (var toAddress in toAddresses)
        {
            logger.LogTrace("Send email: {Subject} to {ToAddress}", substitutedSubject, toAddress.To);
            var msg = new MailMessage
            {
                From = new MailAddress(toAddress.From ?? smtpOptions.DefaultAlarmEmailFromAddress),
                Subject = substitutedSubject,
                Body = substitutedBody
            };
            if (null != toAddress.ReplyTo)
                msg.ReplyToList.Add(toAddress.ReplyTo);
            msg.IsBodyHtml = isBodyHtml;
            msg.To.Add(toAddress.To);
            messengerService.SendSmtpAsync(msg, triggerDto);
        }
    }
    // ReSharper disable once UnusedParameter.Local
    private void SendWebService()
    {
        logger.LogDebug("Sending alarm via WebService");
        // TODO: Write me.  For now, leave blank.
    }

    /// <summary>
    ///     Our Manager has noted that a SMS send has completed on us.  Update any state we need to.
    /// </summary>
    private void NoteSmsSendCompleted()
    {
        // TODO: Log that the send completed.
    }
    private void NoteSmtpSendCompleted()
    {
        // TODO: Log that the send completed.
    }
    private void NoteWebServiceSendCompleted()
    {
        throw new NotImplementedException();
    }

}