using Domain;
using Domain.Enums;

namespace Application.TriggerTypes
{
    public class TriggerTypeService : ITriggerTypeService
    {
        private readonly ITriggerTypeRepository _repository;

        public TriggerTypeService(ITriggerTypeRepository repository)
        {
            _repository = repository;
        }

        public Task<TriggerType?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<TriggerType?> GetByCodeAsync(TriggerTypeCode code) => _repository.GetByCodeAsync(code);
        public Task<List<TriggerType>> GetAllAsync() => _repository.GetAllAsync();
        public Task<TriggerType> AddAsync(TriggerType triggerType) => _repository.AddAsync(triggerType);
        public Task<bool> UpdateAsync(TriggerType triggerType) => _repository.UpdateAsync(triggerType);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
