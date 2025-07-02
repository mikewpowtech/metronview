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
    }
}