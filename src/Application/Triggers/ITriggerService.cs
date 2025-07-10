using Domain;
using Domain.Enums;

namespace Application.Triggers
{
    public interface ITriggerService
    {
        Task<Trigger?> GetByIdAsync(int id);
        Task<List<Trigger>> GetAllAsync();
        Task<List<Trigger>> GetByAlarmIdAsync(int alarmId);
        Task<List<Trigger>> GetByTriggerTypeCodeAsync(TriggerTypeCode triggerTypeCode);
        Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId);
        Task<Trigger> AddAsync(Trigger trigger);
        Task<bool> UpdateAsync(Trigger trigger);
        Task<bool> DeleteAsync(int id);
        Task<List<Trigger>> GetEnabledTriggersAsync();
    }
}
