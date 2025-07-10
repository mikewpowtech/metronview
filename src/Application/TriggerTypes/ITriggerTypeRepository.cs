using Domain;
using Domain.Enums;

namespace Application.TriggerTypes
{
    public interface ITriggerTypeRepository
    {
        Task<TriggerType?> GetByIdAsync(int id);
        Task<TriggerType?> GetByCodeAsync(TriggerTypeCode code);
        Task<List<TriggerType>> GetAllAsync();
        Task<TriggerType> AddAsync(TriggerType triggerType);
        Task<bool> UpdateAsync(TriggerType triggerType);
        Task<bool> DeleteAsync(int id);
    }
}
