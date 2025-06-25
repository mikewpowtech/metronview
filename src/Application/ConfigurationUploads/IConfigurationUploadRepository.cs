using Domain;

namespace Application.ConfigurationUploads;

public interface IConfigurationUploadRepository
{
    Task<List<ConfigurationUpload>> GetAllAsync();
    Task<ConfigurationUpload?> GetByIdAsync(int id);
    Task<List<ConfigurationUpload>> GetByUnitIdAsync(int unitId);
    Task AddAsync(ConfigurationUpload upload);
    // Add more methods as needed
}