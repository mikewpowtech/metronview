using Domain;
using Domain.Enums;

namespace Application.Triggers
{
    public class TriggerService : ITriggerService
    {
        private readonly ITriggerRepository repository;

        public TriggerService(ITriggerRepository repository)
        {
            this.repository = repository;
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

        public void NoteAlarmTrigger(BreachedTriggerDto alarm, bool triggered)
        {
            // This method would typically:
            // 1. Log the alarm trigger event
            // 2. Update the MostRecentAlarms table with the trigger timestamp
            // 3. Possibly create an audit record
            
            // For now, implementing basic functionality
            // You might want to add this to the repository interface if it needs database operations
            
            throw new NotImplementedException("NoteAlarmTrigger method needs to be implemented with proper database operations");
        }

        public void NoteAlarmNotTriggered(BreachedTriggerDto alarm)
        {
            // This method would typically:
            // 1. Log that the alarm was evaluated but not triggered
            // 2. Possibly update statistics or audit logs
            // 3. Clear any pending alarm states if needed
            
            throw new NotImplementedException("NoteAlarmNotTriggered method needs to be implemented with proper logging/audit operations");
        }

        public void AcknowledgeProcessing(int alarmTriggerId)
        {
            // This method would typically:
            // 1. Mark an alarm as processed/acknowledged
            // 2. Update the database to reflect the acknowledgment
            // 3. Possibly send notifications about the acknowledgment
            
            throw new NotImplementedException("AcknowledgeProcessing method needs to be implemented with proper database operations");
        }
    }
}
