using Domain;

namespace Application.Triggers
{
    public interface ITriggerRepository
    {
        Task<Trigger?> GetByIdAsync(int id);
        Task<List<Trigger>> GetAllAsync();
        Task<List<Trigger>> GetByAlarmIdAsync(int alarmId);
        Task<List<Trigger>> GetByTriggerTypeIdAsync(int triggerTypeId);
        Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId);
        Task<Trigger> AddAsync(Trigger trigger);
        Task<bool> UpdateAsync(Trigger trigger);
        Task<bool> DeleteAsync(int id);
        Task<List<Trigger>> GetEnabledTriggersAsync();
    }
}
