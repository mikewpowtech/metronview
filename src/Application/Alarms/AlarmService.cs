using Application.Alarms.Dtos;
using Application.Triggers;
using Domain;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Alarms
{
    public class AlarmService : IAlarmService
    {
        private readonly IAlarmRepository repository;
        private readonly ITriggerRepository triggerRepository;
        private readonly ILogger<AlarmService> logger;

        public AlarmService(IAlarmRepository repository,ITriggerRepository triggerRepository, ILogger<AlarmService> logger)
        {
            this.repository = repository;
            this.triggerRepository = triggerRepository;
            this.logger = logger;
        }

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

        public void SendAlarm(BreachedTriggerDto alarm)
        {
            throw new NotImplementedException();
        }
    }
}