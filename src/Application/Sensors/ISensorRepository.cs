using Domain;

namespace Application.Sensors;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(int id);
    Task<List<Sensor>> GetByUnitIdAsync(string unitId);
    Task<List<Sensor>> GetAllAsync();
    Task<Sensor> AddAsync(Sensor sensor);
    Task<bool> UpdateAsync(Sensor sensor);
    Task<bool> DeleteAsync(int id);
}