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
        public Task<List<Trigger>> GetByTriggerTypeCodeAsync(TriggerTypeCode triggerTypeCode) => repository.GetByTriggerTypeCodeAsync(triggerTypeCode);
        public Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId) => repository.GetByCommunicationModeIdAsync(communicationModeId);
        public Task<Trigger> AddAsync(Trigger trigger) => repository.AddAsync(trigger);
        public Task<bool> UpdateAsync(Trigger trigger) => repository.UpdateAsync(trigger);
        public Task<bool> DeleteAsync(int id) => repository.DeleteAsync(id);
        public Task<List<Trigger>> GetEnabledTriggersAsync() => repository.GetEnabledTriggersAsync();

        public bool IsQuenched(BreachedTriggerDto alarm)
        {
            throw new NotImplementedException();
        }

        public void NoteAlarmTrigger(BreachedTriggerDto alarm, bool v)
        {
            throw new NotImplementedException();
        }

        public void NoteAlarmNotTriggered(BreachedTriggerDto alarm)
        {
            throw new NotImplementedException();
        }

        public void AcknowledgeProcessing(int value)
        {
            throw new NotImplementedException();
        }

        public Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync(TriggerTypeCode notReportedForPeriod)
        {
            throw new NotImplementedException();
        }
    }
}
