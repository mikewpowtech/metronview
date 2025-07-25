using Application.Triggers;
using Domain;
using System.Data;

namespace Application.Recipients
{
    public class RecipientService(IRecipientRepository repository) : IRecipientService
    {
        public Task<Recipient?> GetByIdAsync(int id) => repository.GetByIdAsync(id);
        public Task<List<Recipient>> GetAllAsync() => repository.GetAllAsync();
        public Task<Recipient> AddAsync(Recipient recipient) => repository.AddAsync(recipient);
        public Task<bool> UpdateAsync(Recipient recipient) => repository.UpdateAsync(recipient);
        public Task<bool> DeleteAsync(int id) => repository.DeleteAsync(id);


        public async Task<IEnumerable<RecipientTemplates>> GetRecipientTemplatesAsync(BreachedTriggerDto triggerDto)
       {
            return await repository.GetRecipientTemplatesAsync(triggerDto);
        }
    }
}