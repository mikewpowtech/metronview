using Domain;

namespace Application.ConfigurationUploads;

public interface IConfigurationUploadService
{
    Task<List<ConfigurationUpload>> GetAllAsync();
    Task<ConfigurationUpload?> GetByIdAsync(int id);
    Task<List<ConfigurationUpload>> GetByUnitIdAsync(int unitId);
    Task AddAsync(ConfigurationUpload upload);
    // Add more methods as needed
}