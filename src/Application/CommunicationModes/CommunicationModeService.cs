using Domain;

namespace Application.CommunicationModes
{
    public class CommunicationModeService : ICommunicationModeService
    {
        private readonly ICommunicationModeRepository _repository;

        public CommunicationModeService(ICommunicationModeRepository repository)
        {
            _repository = repository;
        }

        public Task<CommunicationMode?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<CommunicationMode?> GetByCodeAsync(string code) => _repository.GetByCodeAsync(code);
        public Task<List<CommunicationMode>> GetAllAsync() => _repository.GetAllAsync();
        public Task<CommunicationMode> AddAsync(CommunicationMode communicationMode) => _repository.AddAsync(communicationMode);
        public Task<bool> UpdateAsync(CommunicationMode communicationMode) => _repository.UpdateAsync(communicationMode);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
