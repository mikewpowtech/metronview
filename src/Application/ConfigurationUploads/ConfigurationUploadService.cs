using Domain;

namespace Application.ConfigurationUploads;

public class ConfigurationUploadService : IConfigurationUploadService
{
    private readonly IConfigurationUploadRepository _repository;

    public ConfigurationUploadService(IConfigurationUploadRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ConfigurationUpload>> GetAllAsync() => _repository.GetAllAsync();

    public Task<ConfigurationUpload?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<List<ConfigurationUpload>> GetByUnitIdAsync(int unitId) => _repository.GetByUnitIdAsync(unitId);

    public Task AddAsync(ConfigurationUpload upload) => _repository.AddAsync(upload);
}