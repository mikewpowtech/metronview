using Application.Alarms.Dtos;
using Application.Messaging;
using Application.Options;
using Application.Recipients;
using Application.Sensors;
using Application.Triggers;
using Domain;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Substituter;
using System.Net.Mail;

namespace Application.Alarms
{
    public class AlarmService(IOptions<SmtpOptions> smtpOptions,IAlarmRepository repository, ITriggerRepository triggerRepository,
        IRecipientService recipientService, ISensorService sensorService,
        IMessengerService messengerService, ILogger<AlarmService> logger) : IAlarmService
    {
        private readonly SmtpOptions smtpOptions = smtpOptions.Value;

        public Task<Alarm?> GetByIdAsync(int id) => repository.GetByIdAsync(id);
        public Task<List<Alarm>> GetAllAsync() => repository.GetAllAsync();
        public Task<Alarm> AddAsync(Alarm alarm) => repository.AddAsync(alarm);
        public Task<bool> UpdateAsync(Alarm alarm) => repository.UpdateAsync(alarm);
        public Task<bool> DeleteAsync(int id) => repository.DeleteAsync(id);

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
        public Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync() => triggerRepository.GetNotReportedBreachesAsync();


        private bool IsFalling(BreachedTriggerDto alarm)
        {
            logger.LogDebug("Is Reading {Value} falling below {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
            if (double.IsNaN(alarm.Value) || double.IsNaN(alarm.TriggerValue) || alarm.Value > alarm.TriggerValue)
                return false;

            // TODO: You'll need to implement logic to get previous reading
            // This was calling telemetryDatabase.GetMostRecentReadingBefore in the original code
            logger.LogDebug("IsFalling logic needs to be implemented");
            return false;
        }

        private bool IsRising(BreachedTriggerDto alarm)
        {
            logger.LogDebug("Is Reading {Value} rising past {AlarmValue}? ", alarm.Value, alarm.TriggerValue);
            if (double.IsNaN(alarm.Value) || double.IsNaN(alarm.TriggerValue) || alarm.Value < alarm.TriggerValue)
                return false;

            // TODO: You'll need to implement logic to get previous reading
            // This was calling telemetryDatabase.GetMostRecentReadingBefore in the original code
            logger.LogDebug("IsRising logic needs to be implemented");
            return false;
        }

        public async void SendAlarm(BreachedTriggerDto trigger)
        {
            switch (CommunicationModeExtensions.ToAlarmRecipientType(trigger.CommunicationMode))
            {
                case AlarmRecipientType.SMS:
                    await SendSmsAsync(trigger);
                    break;
                case AlarmRecipientType.Email:
                    await SendEmailAsync(trigger);
                    break;
                case AlarmRecipientType.WebService:
                    SendWebService();
                    break;
                case AlarmRecipientType.Unknown:
                default:
                    break;
            }
        }

        private async Task SendSmsAsync(BreachedTriggerDto triggerDto)
        {
            logger.LogDebug("Sending alarm via SMS");
            // There are two sets of substitutions going on here:
            // 1) Substitute the RTU/reading/... values into the alarm's body to produce {message} for the per-company SMS templates;
            var substitutedAlarmBody = sensorService.Substitute(triggerDto, triggerDto.Body);
            var substitutes = new Dictionary<string, string> { { "message", substitutedAlarmBody } };

            // 2) Substitute {sms} and {message} into the per-company SMS templates and send the resulting message.
            // Each recipient may be in a different company with their own set of templates, so handle each one as a single message.
            var toAddresses = await recipientService.GetRecipientTemplatesAsync(triggerDto);
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
        private async Task SendEmailAsync(BreachedTriggerDto triggerDto)
        {
            logger.LogDebug("Sending alarm via Email");
            var substitutedSubject = sensorService.Substitute(triggerDto, triggerDto.Subject);
            var substitutedBody = sensorService.Substitute(triggerDto, triggerDto.Body);
            var isBodyHtml = substitutedBody.ToLower().Contains("<html"); // TODO: Improve this rather unpleasant hack
            var recipentTemplates = await recipientService.GetRecipientTemplatesAsync(triggerDto);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Sending email to {@Recipients}", recipentTemplates.Select(_ => _.Email));
            }

            foreach (var recipentTemplate in recipentTemplates)
            {
                logger.LogTrace("Send email: {Subject} to {ToAddress}", substitutedSubject, recipentTemplate.Email);
                var msg = new MailMessage
                {
                    From = new MailAddress(recipentTemplate.FromAddress ?? smtpOptions.DefaultAlarmEmailFromAddress),
                    Subject = substitutedSubject,
                    Body = substitutedBody
                };
                if (null != recipentTemplate.ReplyToAddress)
                    msg.ReplyToList.Add(recipentTemplate.ReplyToAddress);
                msg.IsBodyHtml = isBodyHtml;
                msg.To.Add(recipentTemplate.Email);
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
            //throw new NotImplementedException();
        }
        private void NoteSmtpSendCompleted()
        {
            // TODO: Log that the send completed.
            //throw new NotImplementedException();
        }
        private void NoteWebServiceSendCompleted()
        {
            throw new NotImplementedException();
        }
    }
}