using Domain;

namespace Application.RecipientSets
{
    public class RecipientSetService : IRecipientSetService
    {
        private readonly IRecipientSetRepository recipientSetRepository;

        public RecipientSetService(IRecipientSetRepository repository)
        {
            recipientSetRepository = repository;
        }

        public Task<RecipientSet?> GetByIdAsync(int id) => recipientSetRepository.GetByIdAsync(id);
        public Task<List<RecipientSet>> GetAllAsync() => recipientSetRepository.GetAllAsync();
        public Task<RecipientSet> AddAsync(RecipientSet recipientSet) => recipientSetRepository.AddAsync(recipientSet);
        public Task<bool> UpdateAsync(RecipientSet recipientSet) => recipientSetRepository.UpdateAsync(recipientSet);
        public Task<bool> DeleteAsync(int id) => recipientSetRepository.DeleteAsync(id);
        
        public async Task<List<Recipient>> GetRecipientsByRecipientSetIdAsync(int recipientSetId)
        {
            var recipients = await recipientSetRepository.GetRecipientsByRecipientSetIdAsync(recipientSetId);

            if (recipients == null || !recipients.Any())
            {
                // Return empty list instead of throwing exception for better UX
                return new List<Recipient>();
            }

            return recipients;
        }

        public Task<bool> AddRecipientToSetAsync(int recipientSetId, int recipientId) => 
            recipientSetRepository.AddRecipientToSetAsync(recipientSetId, recipientId);

        public Task<bool> RemoveRecipientFromSetAsync(int recipientSetId, int recipientId) => 
            recipientSetRepository.RemoveRecipientFromSetAsync(recipientSetId, recipientId);
    }
}