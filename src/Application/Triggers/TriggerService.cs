using Domain;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Triggers
{
    public class TriggerService : ITriggerService
    {
        private readonly ITriggerRepository repository;
        private readonly ILogger<TriggerService> logger;

        public TriggerService(ITriggerRepository repository, ILogger<TriggerService> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public Task<Trigger?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

        public Task<List<Trigger>> GetAllAsync() => repository.GetAllAsync();

        public Task<List<Trigger>> GetByAlarmIdAsync(int alarmId) => repository.GetByAlarmIdAsync(alarmId);

        public Task<List<Trigger>> GetByTriggerTypeCodeAsync(TriggerTypeCode triggerTypeCode) => 
            repository.GetByTriggerTypeCodeAsync(triggerTypeCode);

        public Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId) => 
            repository.GetByCommunicationModeIdAsync(communicationModeId);

        public Task<Trigger> AddAsync(Trigger trigger) => repository.AddAsync(trigger);

        public Task<bool> UpdateAsync(Trigger trigger) => repository.UpdateAsync(trigger);

        public Task<bool> DeleteAsync(int id) => repository.DeleteAsync(id);

        public Task<List<Trigger>> GetEnabledTriggersAsync() => repository.GetEnabledTriggersAsync();

        public Task<bool> IsQuenchedAsync(BreachedTriggerDto alarm)
        {
            return repository.IsQuenchedAsync(alarm);
        }

        public Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync()
        {
            return repository.GetNotReportedBreachesAsync();
        }

        public async Task NoteAlarmTriggerAsync(BreachedTriggerDto alarm, bool triggered)
        {
            logger.LogDebug("Noting alarm trigger for SensorId: {SensorId}, AlarmId: {AlarmSetId}, Triggered: {Triggered}", 
                alarm.SensorId, alarm.AlarmSetId, triggered);

            if (triggered)
            {
                // Update the MostRecentAlarms table with the current timestamp
                await repository.AddOrUpdateMostRecentAlarmAsync(
                    alarm.SensorId, 
                    alarm.AlarmSetId, 
                    DateTime.UtcNow);

                logger.LogInformation("Alarm triggered and recorded for SensorId: {SensorId}, AlarmId: {AlarmSetId}", 
                    alarm.SensorId, alarm.AlarmSetId);
            }
            else
            {
                logger.LogDebug("Alarm was evaluated but not triggered for SensorId: {SensorId}, AlarmId: {AlarmSetId}", 
                    alarm.SensorId, alarm.AlarmSetId);
            }
        }

        public void NoteAlarmNotTriggered(BreachedTriggerDto alarm)
        {
            // This method would typically:
            // 1. Log that the alarm was evaluated but not triggered
            // 2. Possibly update statistics or audit logs
            // 3. Clear any pending alarm states if needed
            
            logger.LogDebug("Alarm not triggered for SensorId: {SensorId}, AlarmId: {AlarmSetId}", 
                alarm.SensorId, alarm.AlarmSetId);
        }

        public void AcknowledgeProcessing(int alarmTriggerId)
        {
            // This method would typically:
            // 1. Mark an alarm as processed/acknowledged
            // 2. Update the database to reflect the acknowledgment
            // 3. Possibly send notifications about the acknowledgment
            
            logger.LogDebug("Acknowledging processing for AlarmTriggerId: {AlarmTriggerId}", alarmTriggerId);
        }
    }
}
