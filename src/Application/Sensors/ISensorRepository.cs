using Domain;

namespace Application.Sensors;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(string id);
    Task<List<Sensor>> GetAllAsync();
    Task<Sensor> AddAsync(Sensor sensor);
    Task<bool> UpdateAsync(Sensor sensor);
    Task<bool> DeleteAsync(string id);
}