using Domain;

namespace Application.TriggerTypes
{
    public interface ITriggerTypeService
    {
        Task<TriggerType?> GetByIdAsync(int id);
        Task<TriggerType?> GetByCodeAsync(string code);
        Task<List<TriggerType>> GetAllAsync();
        Task<TriggerType> AddAsync(TriggerType triggerType);
        Task<bool> UpdateAsync(TriggerType triggerType);
        Task<bool> DeleteAsync(int id);
    }
}
