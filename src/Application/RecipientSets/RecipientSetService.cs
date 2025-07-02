using Domain;

namespace Application.RecipientSets
{
    public class RecipientSetService : IRecipientSetService
    {
        private readonly IRecipientSetRepository _repository;

        public RecipientSetService(IRecipientSetRepository repository)
        {
            _repository = repository;
        }

        public Task<RecipientSet?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<List<RecipientSet>> GetAllAsync() => _repository.GetAllAsync();
        public Task<RecipientSet> AddAsync(RecipientSet recipientSet) => _repository.AddAsync(recipientSet);
        public Task<bool> UpdateAsync(RecipientSet recipientSet) => _repository.UpdateAsync(recipientSet);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}