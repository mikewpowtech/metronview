using Domain;

namespace Application.CommunicationModes
{
    public interface ICommunicationModeService
    {
        Task<CommunicationMode?> GetByIdAsync(int id);
        Task<CommunicationMode?> GetByCodeAsync(string code);
        Task<List<CommunicationMode>> GetAllAsync();
        Task<CommunicationMode> AddAsync(CommunicationMode communicationMode);
        Task<bool> UpdateAsync(CommunicationMode communicationMode);
        Task<bool> DeleteAsync(int id);
    }
}
