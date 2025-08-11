using Domain;

namespace Application.RecipientSets
{
    public interface IRecipientSetRepository
    {
        Task<RecipientSet?> GetByIdAsync(int id);
        Task<List<RecipientSet>> GetAllAsync();
        Task<RecipientSet> AddAsync(RecipientSet recipientSet);
        Task<bool> UpdateAsync(RecipientSet recipientSet);
        Task<bool> DeleteAsync(int id);
        Task<List<Recipient>> GetRecipientsByRecipientSetIdAsync(int recipientSetId);
        Task<bool> AddRecipientToSetAsync(int recipientSetId, int recipientId);
        Task<bool> RemoveRecipientFromSetAsync(int recipientSetId, int recipientId);
    }
}