using Domain;
using Domain.Enums;

namespace Application.Triggers
{
    public class TriggerService : ITriggerService
    {
        private readonly ITriggerRepository _repository;

        public TriggerService(ITriggerRepository repository)
        {
            _repository = repository;
        }

        public Task<Trigger?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<List<Trigger>> GetAllAsync() => _repository.GetAllAsync();
        public Task<List<Trigger>> GetByAlarmIdAsync(int alarmId) => _repository.GetByAlarmIdAsync(alarmId);
        public Task<List<Trigger>> GetByTriggerTypeCodeAsync(TriggerTypeCode triggerTypeCode) => _repository.GetByTriggerTypeCodeAsync(triggerTypeCode);
        public Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId) => _repository.GetByCommunicationModeIdAsync(communicationModeId);
        public Task<Trigger> AddAsync(Trigger trigger) => _repository.AddAsync(trigger);
        public Task<bool> UpdateAsync(Trigger trigger) => _repository.UpdateAsync(trigger);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
        public Task<List<Trigger>> GetEnabledTriggersAsync() => _repository.GetEnabledTriggersAsync();
    }
}
