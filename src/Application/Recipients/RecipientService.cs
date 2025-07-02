using Domain;

namespace Application.Recipients
{
    public class RecipientService : IRecipientService
    {
        private readonly IRecipientRepository _repository;

        public RecipientService(IRecipientRepository repository)
        {
            _repository = repository;
        }

        public Task<Recipient?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<List<Recipient>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Recipient> AddAsync(Recipient recipient) => _repository.AddAsync(recipient);
        public Task<bool> UpdateAsync(Recipient recipient) => _repository.UpdateAsync(recipient);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}