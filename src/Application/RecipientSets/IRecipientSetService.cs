using Domain;

namespace Application.RecipientSets
{
    public interface IRecipientSetService
    {
        Task<RecipientSet?> GetByIdAsync(int id);
        Task<List<RecipientSet>> GetAllAsync();
        Task<RecipientSet> AddAsync(RecipientSet recipientSet);
        Task<bool> UpdateAsync(RecipientSet recipientSet);
        Task<bool> DeleteAsync(int id);
        Task<List<Recipient>> GetRecipientsByRecipientSetIdAsync(int recipientSetId);
    }
}