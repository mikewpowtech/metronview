using Domain;

namespace Application.Recipients
{
    public interface IRecipientService
    {
        Task<Recipient?> GetByIdAsync(int id);
        Task<List<Recipient>> GetAllAsync();
        Task<Recipient> AddAsync(Recipient recipient);
        Task<bool> UpdateAsync(Recipient recipient);
        Task<bool> DeleteAsync(int id);
    }
}